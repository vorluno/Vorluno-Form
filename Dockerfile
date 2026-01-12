# ===== STAGE 1: Build Frontend (Node.js + Vite) =====
FROM node:18-alpine AS frontend-build
WORKDIR /src/web

# Install frontend dependencies
COPY src/web/package*.json ./
RUN npm ci

# Copy frontend source and build
COPY src/web/ ./
RUN npm run build

# ===== STAGE 2: Build Backend (.NET 9 + Node) =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src/api

# Install Node.js for backend build process
RUN apt-get update \
    && apt-get install -y nodejs npm \
    && rm -rf /var/lib/apt/lists/*

# Copy backend project files
COPY src/api/*.csproj ./
RUN dotnet restore

# Copy all backend source
COPY src/api/ ./

# Copy built frontend assets to wwwroot
COPY --from=frontend-build /src/web/dist ./wwwroot

# Publish backend
RUN dotnet publish -c Release -o /app/publish

# ===== STAGE 3: Runtime (.NET 9) =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published app
COPY --from=backend-build /app/publish .

# Set environment
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 80

ENTRYPOINT ["dotnet", "Vorluno.Contacto.Api.dll"]
