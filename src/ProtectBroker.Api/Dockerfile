# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder

WORKDIR /src

# Copy project files
COPY ProtectBroker.Api.csproj ./ProtectBroker.Api/
COPY ../ProtectBroker.Core/ProtectBroker.Core.csproj ./ProtectBroker.Core/
COPY ../ProtectBroker.Infrastructure/ProtectBroker.Infrastructure.csproj ./ProtectBroker.Infrastructure/

WORKDIR /src/ProtectBroker.Api

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build application
RUN dotnet build -c Release --no-restore

# Publish application
RUN dotnet publish -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine

WORKDIR /app

# Install additional tools (useful for debugging)
RUN apk add --no-cache tini curl

# Copy published application
COPY --from=builder /app/publish .

# Create non-root user for security
RUN addgroup -g 1000 -S dotnet && \
    adduser -u 1000 -S dotnet -G dotnet && \
    chown -R dotnet:dotnet /app

USER dotnet

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Use tini for proper signal handling
ENTRYPOINT ["/sbin/tini", "--"]
CMD ["dotnet", "ProtectBroker.Api.dll"]
