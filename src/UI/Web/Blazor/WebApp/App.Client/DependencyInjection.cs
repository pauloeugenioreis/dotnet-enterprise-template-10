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
        services.AddTransient<LoadingHandler>();
        services.AddTransient<AuthenticationHandler>();
        services.AddTransient<ErrorHandler>();

        var httpClientBuilder = services.AddHttpClient("ApiGateway", client => 
            client.BaseAddress = new Uri(apiUrl))
            .AddHttpMessageHandler<LoadingHandler>()
            .AddHttpMessageHandler<AuthenticationHandler>()
            .AddHttpMessageHandler<ErrorHandler>();

        if (!OperatingSystem.IsBrowser())
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
        }

        services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiGateway"));

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
