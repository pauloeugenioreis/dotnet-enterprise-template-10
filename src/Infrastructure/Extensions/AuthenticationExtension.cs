using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Interfaces;
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
        // Build a temporary service provider to get AppSettings
        var serviceProvider = services.BuildServiceProvider();
        var appSettings = serviceProvider.GetRequiredService<AppSettings>();
        var authSettings = appSettings.Authentication;

        if (!authSettings.Enabled)
        {
            Console.WriteLine("⚠️  Authentication is disabled");
            return services;
        }

        // Validate JWT Secret
        if (string.IsNullOrWhiteSpace(authSettings.Jwt.Secret))
        {
            throw new InvalidOperationException(
                "JWT Secret is not configured. Please set 'AppSettings:Authentication:Jwt:Secret' in appsettings.json with a secure key of at least 32 characters.");
        }

        if (authSettings.Jwt.Secret.Length < 32)
        {
            throw new InvalidOperationException(
                $"JWT Secret must be at least 32 characters long for security. Current length: {authSettings.Jwt.Secret.Length}");
        }

        // Register authentication services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, JwtTokenService>();

        // Configure JWT Bearer Authentication
        var key = Encoding.ASCII.GetBytes(authSettings.Jwt.Secret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Set to true in production
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = authSettings.Jwt.ValidateIssuer,
                ValidateAudience = authSettings.Jwt.ValidateAudience,
                ValidateLifetime = authSettings.Jwt.ValidateLifetime,
                ValidateIssuerSigningKey = authSettings.Jwt.ValidateIssuerSigningKey,
                ValidIssuer = authSettings.Jwt.Issuer,
                ValidAudience = authSettings.Jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        error = "Unauthorized",
                        message = "You are not authorized to access this resource"
                    });
                    return context.Response.WriteAsync(result);
                }
            };
        });

        // Configure OAuth2 providers if enabled
        // Note: OAuth2 providers require additional NuGet packages:
        // - Google: Microsoft.AspNetCore.Authentication.Google
        // - Microsoft: Microsoft.AspNetCore.Authentication.MicrosoftAccount
        // - GitHub: AspNet.Security.OAuth.GitHub
        /*
        if (authSettings.OAuth2.Enabled)
        {
            var authBuilder = services.AddAuthentication();

            // Google OAuth2
            if (authSettings.OAuth2.Google.Enabled)
            {
                authBuilder.AddGoogle(options =>
                {
                    options.ClientId = authSettings.OAuth2.Google.ClientId;
                    options.ClientSecret = authSettings.OAuth2.Google.ClientSecret;
                });
                Console.WriteLine("✅ Google OAuth2 enabled");
            }

            // Microsoft OAuth2
            if (authSettings.OAuth2.Microsoft.Enabled)
            {
                authBuilder.AddMicrosoftAccount(options =>
                {
                    options.ClientId = authSettings.OAuth2.Microsoft.ClientId;
                    options.ClientSecret = authSettings.OAuth2.Microsoft.ClientSecret;
                    if (!string.IsNullOrEmpty(authSettings.OAuth2.Microsoft.TenantId))
                    {
                        options.AuthorizationEndpoint = $"https://login.microsoftonline.com/{authSettings.OAuth2.Microsoft.TenantId}/oauth2/v2.0/authorize";
                        options.TokenEndpoint = $"https://login.microsoftonline.com/{authSettings.OAuth2.Microsoft.TenantId}/oauth2/v2.0/token";
                    }
                });
                Console.WriteLine("✅ Microsoft OAuth2 enabled");
            }

            // GitHub OAuth2
            if (authSettings.OAuth2.GitHub.Enabled)
            {
                authBuilder.AddGitHub(options =>
                {
                    options.ClientId = authSettings.OAuth2.GitHub.ClientId;
                    options.ClientSecret = authSettings.OAuth2.GitHub.ClientSecret;
                });
                Console.WriteLine("✅ GitHub OAuth2 enabled");
            }
        }
        */

        services.AddAuthorization();

        Console.WriteLine("✅ JWT Authentication enabled");
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
