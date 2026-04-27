using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using BlazorApp.Client.Services.Base;
using BlazorApp.Client.Services;
using BlazorApp.Client.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using BlazorApp.Client.Services.Handlers;

namespace BlazorApp.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddClientServices(this IServiceCollection services, string apiUrl)
    {
        services.AddAuthorizationCore();
        services.AddCascadingAuthenticationState();
        services.AddMudServices();
        
        services.AddScoped<LoadingService>();
        services.AddScoped<TokenProvider>();
        services.AddScoped<LoadingHandler>();
        services.AddScoped<AuthenticationHandler>();
        services.AddScoped<ErrorHandler>();

        // Registro base para o IHttpClientFactory obter as configurações do Aspire (Resilience, Service Discovery, etc.)
        var httpClientBuilder = services.AddHttpClient("ApiGatewayInternal", client => 
            client.BaseAddress = new Uri(apiUrl));

        if (!OperatingSystem.IsBrowser())
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        }

        // HttpClient Scoped para o Circuito Blazor Server
        services.AddScoped(sp => 
        {
            if (OperatingSystem.IsBrowser())
            {
                return sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiGatewayInternal");
            }

            // No Blazor Server, resolvemos os handlers do container do circuito
            var loadingHandler = sp.GetRequiredService<LoadingHandler>();
            var authHandler = sp.GetRequiredService<AuthenticationHandler>();
            var errorHandler = sp.GetRequiredService<ErrorHandler>();
            
            // Obtemos a pipeline base do Aspire (inclui Resilience, etc.)
            var handlerFactory = sp.GetRequiredService<IHttpMessageHandlerFactory>();
            var baseHandler = handlerFactory.CreateHandler("ApiGatewayInternal");

            // Montamos a pipeline: Loading -> Auth -> Error -> Aspire(Resilience/Primary)
            loadingHandler.InnerHandler = authHandler;
            authHandler.InnerHandler = errorHandler;
            errorHandler.InnerHandler = baseHandler;

            return new HttpClient(loadingHandler)
            {
                BaseAddress = new Uri(apiUrl)
            };
        });

        // Core Services
        services.AddScoped<LocalStorageService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerReviewService, CustomerReviewService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IAuditService, AuditService>();
        
        // Auth Provider
        services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

        return services;
    }
}
