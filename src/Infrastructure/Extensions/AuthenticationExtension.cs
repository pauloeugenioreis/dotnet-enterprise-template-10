using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectTemplate.Domain;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Extension methods for Authentication and Authorization
/// Provides JWT Bearer authentication and policy-based authorization
/// </summary>
public static class AuthenticationExtension
{
    /// <summary>
    /// Adds JWT Authentication and Authorization services
    /// Configure in appsettings.json under AppSettings:Authentication
    /// </summary>
    public static IServiceCollection AddAuthentication Extension(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var sp = services.BuildServiceProvider();
            var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
            var jwtSettings = appSettings.Authentication?.Jwt;

            if (jwtSettings != null)
            {
                options.RequireHttpsMetadata = appSettings.Infrastructure?.Environment != "Development";
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrWhiteSpace(jwtSettings.Issuer),
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(jwtSettings.Audience),
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret ?? throw new InvalidOperationException("JWT Secret is required"))),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            }

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    var logger = ctx.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("JwtBearer");
                    logger.LogError(ctx.Exception, "JWT authentication failed.");
                    return Task.CompletedTask;
                },
                OnTokenValidated = ctx =>
                {
                    var logger = ctx.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("JwtBearer");
                    logger.LogInformation("JWT token validated for user: {User}", 
                        ctx.Principal?.Identity?.Name ?? "Unknown");
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            // Default policy requires authentication
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Add custom policies here
            // Example: options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        });

        return services;
    }
}
