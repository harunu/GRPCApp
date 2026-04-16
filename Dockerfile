# Multi-stage build for production-ready gRPC service
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src

# Copy solution and project files
COPY ["BlazorGrpcWebApp/BlazorGrpcWebApp.sln", "."]
COPY ["BlazorGrpcWebApp/GrpcService/GrpcService.csproj", "GrpcService/"]
COPY ["BlazorGrpcWebApp/BlazorGrpcWebApp/Shared/BlazorGrpcWebApp.Shared.csproj", "BlazorGrpcWebApp/Shared/"]

# Restore dependencies
RUN dotnet restore "GrpcService/GrpcService.csproj"

# Copy source code
COPY BlazorGrpcWebApp/GrpcService/. GrpcService/
COPY BlazorGrpcWebApp/BlazorGrpcWebApp/Shared/. BlazorGrpcWebApp/Shared/

# Build application
RUN dotnet build "GrpcService/GrpcService.csproj" -c Release -o /app/build

# Publish application
FROM build AS publish
RUN dotnet publish "GrpcService/GrpcService.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup -g 1001 appuser && \
    adduser -D -u 1001 -G appuser appuser

# Copy published application
COPY --from=publish /app/publish .

# Copy App_Data directory if it exists (for CSV data)
COPY --chown=appuser:appuser BlazorGrpcWebApp/GrpcService/App_Data /app/App_Data || true

# Health check endpoint configuration
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5000/ || exit 1

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 5000 5001

# Set environment variables for production
ENV ASPNETCORE_URLS=https://+:5001;http://+:5000 \
    ASPNETCORE_ENVIRONMENT=Production

# Run application
ENTRYPOINT ["dotnet", "GrpcService.dll"]