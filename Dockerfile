# syntax=docker/dockerfile:1

# ---- Etapa 1: build/publish ----
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY NuGet.Config ./
COPY global.json ./
COPY src/CleanArchitecture.Full.Api/*.csproj src/CleanArchitecture.Full.Api/
COPY src/CleanArchitecture.Full.Application/*.csproj src/CleanArchitecture.Full.Application/
COPY src/CleanArchitecture.Full.Domain/*.csproj src/CleanArchitecture.Full.Domain/
COPY src/CleanArchitecture.Full.Infrastructure/*.csproj src/CleanArchitecture.Full.Infrastructure/
# Restauramos solo el proyecto de la API (y sus ProjectReferences transitivas), no el .sln completo:
# así la imagen no arrastra el proyecto de tests ni sus dependencias.
RUN dotnet restore src/CleanArchitecture.Full.Api/CleanArchitecture.Full.Api.csproj

COPY src/ src/
RUN dotnet publish src/CleanArchitecture.Full.Api/CleanArchitecture.Full.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Etapa 2: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

# krb5-libs: Npgsql intenta negociar GSSAPI/Kerberos al conectar y Alpine no la trae por defecto;
# sin esto igual funciona (cae a autenticación por password) pero deja un ERROR de log falso positivo.
RUN apk add --no-cache krb5-libs

# La imagen base ya incluye un usuario "app" sin privilegios; lo reutilizamos
# en vez de crear uno nuevo (evita colisiones de UID/GID con la imagen).
COPY --from=build /app/publish .
RUN chown -R app:app /app
USER app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CleanArchitecture.Full.Api.dll"]
