# Dockerfile for Blazor WebApp
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/UI/Web/Blazor/WebApp/App/App.csproj", "src/UI/Web/Blazor/WebApp/App/"]
COPY ["src/UI/Web/Blazor/WebApp/App.Client/App.Client.csproj", "src/UI/Web/Blazor/WebApp/App.Client/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
COPY ["src/Aspire/ServiceDefaults/ServiceDefaults.csproj", "src/Aspire/ServiceDefaults/"]
COPY ["src/Server/Domain/Domain.csproj", "src/Server/Domain/"]
RUN dotnet restore "src/UI/Web/Blazor/WebApp/App/App.csproj"
COPY . .
WORKDIR "/src/src/UI/Web/Blazor/WebApp/App"
RUN dotnet build "App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App.dll"]
