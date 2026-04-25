using Microsoft.Extensions.Logging;
using EnterpriseTemplate.MauiApp.Services;
using EnterpriseTemplate.MauiApp.Services.Base;
using EnterpriseTemplate.MauiApp.Pages;

namespace EnterpriseTemplate.MauiApp;

public static class MauiProgram
{
    private const string ApiBaseUrl = "https://localhost:7196";

    public static Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
    {
        var builder = Microsoft.Maui.Hosting.MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddTransient<AuthTokenHandler>();
        builder.Services.AddHttpClient("ApiGateway", client =>
        {
            client.BaseAddress = new Uri(ApiBaseUrl);
        })
        .AddHttpMessageHandler<AuthTokenHandler>();

        builder.Services.AddScoped(sp =>
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiGateway"));

        // ─── Services ──────────────────────────────────────────────────────────
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IDocumentService, DocumentService>();
        builder.Services.AddScoped<ICustomerReviewService, CustomerReviewService>();

        // ─── Pages ─────────────────────────────────────────────────────────────
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<OrdersPage>();
        builder.Services.AddTransient<ProductsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
