FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

# Install curl for Docker healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

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
WORKDIR "/src/src/Server/Api"
RUN dotnet build "Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

# Run as non-root user for security (user 'app' is built-in on .NET 10 images)
USER $APP_UID

WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=5 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Api.dll"]
