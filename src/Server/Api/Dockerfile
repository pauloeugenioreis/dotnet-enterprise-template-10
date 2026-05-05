FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080

# Install curl for Docker healthcheck
RUN apk add --no-cache curl

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

# Copy csproj files and restore
COPY ["src/Server/Api/Api.csproj", "src/Server/Api/"]
COPY ["src/Server/Application/Application.csproj", "src/Server/Application/"]
COPY ["src/Server/Data/Data.csproj", "src/Server/Data/"]
COPY ["src/Server/Domain/Domain.csproj", "src/Server/Domain/"]
COPY ["src/Server/Infrastructure/Infrastructure.csproj", "src/Server/Infrastructure/"]
COPY ["src/Aspire/ServiceDefaults/ServiceDefaults.csproj", "src/Aspire/ServiceDefaults/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

RUN dotnet restore "src/Server/Api/Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/app/src/Server/Api"
RUN dotnet build "Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run as non-root user for security
USER $APP_UID

HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=5 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Api.dll"]
