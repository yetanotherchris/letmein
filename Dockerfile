# Build stage - Frontend assets
FROM node:20-alpine AS frontend-build
WORKDIR /src

# Copy package files for better layer caching
COPY src/Letmein.ReactWeb/package*.json ./
RUN npm ci

# Copy remaining frontend files and build
COPY src/Letmein.ReactWeb/ ./
RUN npm run build

# Build stage - .NET application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (better layer caching)
COPY Directory.Packages.props ./
COPY src/Letmein.Core/Letmein.Core.csproj src/Letmein.Core/
COPY src/Letmein.Api/Letmein.Api.csproj src/Letmein.Api/
RUN dotnet restore src/Letmein.Api/Letmein.Api.csproj

# Copy source files
COPY src/Letmein.Core/ ./src/Letmein.Core/
COPY src/Letmein.Api/ ./src/Letmein.Api/

# Build and publish
RUN dotnet publish src/Letmein.Api/Letmein.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
LABEL maintainer="Chris Small"
LABEL org.opencontainers.image.title="Letmein"
LABEL org.opencontainers.image.description="Encrypted notes service with temporary storage"
LABEL org.opencontainers.image.source="https://github.com/yetanotherchris/letmein"

WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Create storage directory
RUN mkdir -p /app/storage

# Expose port (Kestrel listens on 8080 by default in .NET 9)
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    REPOSITORY_TYPE=FileSystem \
    POSTGRES_CONNECTIONSTRING="" \
    EXPIRY_TIMES=720 \
    CLEANUP_SLEEPTIME=300 \
    ID_TYPE=default

ENTRYPOINT ["dotnet", "Letmein.Api.dll"]
