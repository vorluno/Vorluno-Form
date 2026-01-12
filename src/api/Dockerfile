# ===== STAGE 1: build (.NET 9 + Node) =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1) Instalar Node.js + npm para que el Target de Vite pueda ejecutar "npm ci" y "npm run build"
RUN apt-get update \
    && apt-get install -y nodejs npm \
    && rm -rf /var/lib/apt/lists/*

# 2) Copiar el código del proyecto (carpeta interna)
COPY . .

# 3) Publicar en Release (esto ejecuta tus Targets de SPA dentro del .csproj)
# ⚠️ IMPORTANTE: Especificar el proyecto explícitamente para que los paths relativos
#    en los targets del .csproj funcionen correctamente
RUN dotnet publish ./Vorluno.Contacto.Api.csproj -c Release -o /app/publish

# ===== STAGE 2: runtime (.NET 9) =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Puerto interno donde escucha la app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Ajusta el nombre del dll si fuera distinto
ENTRYPOINT ["dotnet", "Vorluno.Contacto.Api.dll"]
