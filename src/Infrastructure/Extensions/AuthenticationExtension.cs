using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Interfaces;
using ProjectTemplate.Infrastructure.Configuration;
using ProjectTemplate.Infrastructure.Services;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Authentication extension methods for configuring JWT Bearer authentication
/// </summary>
public static class AuthenticationExtension
{
    /// <summary>
    /// Add JWT Authentication services
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        // Register authentication validator
        services.AddSingleton<IValidateOptions<AppSettings>, AuthenticationSettingsValidator>();

        // Register authentication services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, JwtTokenService>();

        // Register JWT Bearer configuration
        services.ConfigureOptions<JwtBearerOptionsSetup>();

        // Add Authentication with JWT Bearer
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(); // Options configured by JwtBearerOptionsSetup

        // Configure OAuth2 providers if enabled
        // Note: OAuth2 providers require additional NuGet packages:
        // - Google: Microsoft.AspNetCore.Authentication.Google
        // - Microsoft: Microsoft.AspNetCore.Authentication.MicrosoftAccount
        // - GitHub: AspNet.Security.OAuth.GitHub
        /*
        var tempProvider = services.BuildServiceProvider();
        var appSettings = tempProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        
        if (appSettings.Authentication.OAuth2.Enabled)
        {
            // Google OAuth2
            if (appSettings.Authentication.OAuth2.Google.Enabled)
            {
                authBuilder.AddGoogle(options =>
                {
                    options.ClientId = appSettings.Authentication.OAuth2.Google.ClientId;
                    options.ClientSecret = appSettings.Authentication.OAuth2.Google.ClientSecret;
                });
                Console.WriteLine("✅ Google OAuth2 enabled");
            }

            // Microsoft OAuth2
            if (appSettings.Authentication.OAuth2.Microsoft.Enabled)
            {
                authBuilder.AddMicrosoftAccount(options =>
                {
                    options.ClientId = appSettings.Authentication.OAuth2.Microsoft.ClientId;
                    options.ClientSecret = appSettings.Authentication.OAuth2.Microsoft.ClientSecret;
                    if (!string.IsNullOrEmpty(appSettings.Authentication.OAuth2.Microsoft.TenantId))
                    {
                        options.AuthorizationEndpoint = $"https://login.microsoftonline.com/{appSettings.Authentication.OAuth2.Microsoft.TenantId}/oauth2/v2.0/authorize";
                        options.TokenEndpoint = $"https://login.microsoftonline.com/{appSettings.Authentication.OAuth2.Microsoft.TenantId}/oauth2/v2.0/token";
                    }
                });
                Console.WriteLine("✅ Microsoft OAuth2 enabled");
            }

            // GitHub OAuth2
            if (appSettings.Authentication.OAuth2.GitHub.Enabled)
            {
                authBuilder.AddGitHub(options =>
                {
                    options.ClientId = appSettings.Authentication.OAuth2.GitHub.ClientId;
                    options.ClientSecret = appSettings.Authentication.OAuth2.GitHub.ClientSecret;
                });
                Console.WriteLine("✅ GitHub OAuth2 enabled");
            }
        }
        */

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Use Authentication middleware
    /// </summary>
    public static IApplicationBuilder UseJwtAuthentication(this IApplicationBuilder app)
    {
        // Skip authentication in Testing environment
        var env = app.ApplicationServices.GetService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        if (env?.EnvironmentName == "Testing")
        {
            return app;
        }
        
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
