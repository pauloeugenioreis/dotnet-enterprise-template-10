using Microsoft.Extensions.DependencyInjection;
using BlazorApp.Client.Services.Base;
using BlazorApp.Client.Services;
using BlazorApp.Client.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddClientServices(this IServiceCollection services, string apiUrl)
    {
        services.AddAuthorizationCore();
        services.AddCascadingAuthenticationState();
        
        services.AddScoped<LoadingService>();
        services.AddTransient<LoadingHandler>();

        services.AddHttpClient("ApiGateway", client => 
            client.BaseAddress = new Uri(apiUrl))
            .AddHttpMessageHandler<LoadingHandler>();

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
