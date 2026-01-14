# ===== STAGE 1: Build Frontend (Node.js + Vite) =====
FROM node:20-alpine AS frontend-build
WORKDIR /src/web

# Install frontend dependencies (including devDependencies for build)
COPY src/web/package*.json ./
RUN npm ci

# Copy frontend source and build
COPY src/web/ ./
RUN npm run build

# ===== STAGE 2: Build Backend (.NET 9) =====
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS backend-build
WORKDIR /src/api

# Copy backend project files
COPY src/api/*.csproj ./
RUN dotnet restore

# Copy all backend source
COPY src/api/ ./

# Copy built frontend assets to wwwroot
COPY --from=frontend-build /src/web/dist ./wwwroot

# Publish backend
RUN dotnet publish -c Release -o /app/publish --no-restore

# ===== STAGE 3: Runtime (.NET 9) =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final

# Install curl for healthcheck
RUN apk add --no-cache curl

WORKDIR /app

# Copy published app
COPY --from=backend-build /app/publish .

# Create non-root user
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser && \
    chown -R appuser:appuser /app

USER appuser

# Set environment (CapRover will inject PORT, default to 8080)
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "Vorluno.Contacto.Api.dll"]
