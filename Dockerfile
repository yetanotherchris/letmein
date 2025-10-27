# Build stage - Frontend assets
FROM node:20-alpine AS frontend-build
WORKDIR /src

# Copy package files for better layer caching
COPY src/Letmein.Web/wwwroot/package*.json ./
RUN npm ci

# Copy remaining frontend files and build
COPY src/Letmein.Web/wwwroot/ ./
RUN npm run build

# Build stage - .NET application
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy csproj files and restore dependencies (better layer caching)
COPY src/Letmein.Core/Letmein.Core.csproj src/Letmein.Core/
COPY src/Letmein.Web/Letmein.Web.csproj src/Letmein.Web/
RUN dotnet restore src/Letmein.Web/Letmein.Web.csproj

# Copy source files (excluding wwwroot for now)
COPY src/Letmein.Core/ ./src/Letmein.Core/
COPY src/Letmein.Web/ ./src/Letmein.Web/

# Copy built frontend assets from previous stage
COPY --from=frontend-build /src/dist/ ./src/Letmein.Web/wwwroot/dist/

# Build and publish
WORKDIR /src/src/Letmein.Web
RUN dotnet publish -c Release -o /app/publish \
    --no-restore \
    --runtime linux-musl-x64 \
    --self-contained false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
LABEL maintainer="Chris Small"
LABEL org.opencontainers.image.title="Letmein"
LABEL org.opencontainers.image.description="Encrypted notes service with temporary storage"
LABEL org.opencontainers.image.source="https://github.com/yetanotherchris/letmein"

WORKDIR /app

# Create non-root user for security
RUN addgroup -g 1001 -S appgroup && \
    adduser -u 1001 -S appuser -G appgroup && \
    mkdir -p /app/storage && \
    chown -R appuser:appgroup /app

# Copy published application
COPY --from=build --chown=appuser:appgroup /app/publish .

# Switch to non-root user
USER appuser

# Expose port (Kestrel listens on 8080 by default in .NET 9)
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    REPOSITORY_TYPE=FileSystem \
    POSTGRES_CONNECTIONSTRING="" \
    EXPIRY_TIMES=720 \
    CLEANUP_SLEEPTIME=300 \
    ID_TYPE=default \
    PAGE_TITLE=letmein.io \
    HEADER_TEXT=letmein.io \
    HEADER_SUBTEXT="note encryption service" \
    FOOTER_TEXT="<a href=\"https://github.com/yetanotherchris/letmein\">Github source</a>&nbsp;|&nbsp;<a href=\"/FAQ\">FAQ</a>"

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "Letmein.Web.dll"]
