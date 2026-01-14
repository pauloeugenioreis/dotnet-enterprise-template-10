using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectTemplate.Domain;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Main infrastructure extension that orchestrates all infrastructure components
/// </summary>
public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Core settings
        services.AddAppSettingsConfiguration(configuration, environment);

        // Database
        services.AddDatabaseConfiguration();

        // Cache
        services.AddCacheConfiguration();

        // Health checks
        services.AddHealthChecksConfiguration();

        // Telemetry (OpenTelemetry)
        services.AddTelemetry(environment);

        // Rate Limiting
        services.AddRateLimitingConfiguration();

        // Event Sourcing
        services.AddEventSourcing();

        // Authentication (JWT + OAuth2)
        services.AddJwtAuthentication();

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCorsPolicy", policy =>
            {
                if (environment.IsDevelopment())
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
                else
                {
                    // Configure production CORS policy
                    policy.WithOrigins("https://yourdomain.com")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
            });
        });

        // Response compression
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        // API Versioning
        services.AddCustomizedApiVersioning();

        // Application dependencies
        services.AddApplicationDependencies();

        return services;
    }

    public static IApplicationBuilder UseInfrastructureMiddleware(this IApplicationBuilder app, IConfiguration configuration)
    {
        // CORS
        app.UseCors("DefaultCorsPolicy");

        // Response compression
        app.UseResponseCompression();

        // Authentication & Authorization
        app.UseJwtAuthentication();

        // Rate Limiting - Only if enabled in configuration
        var serviceProvider = app.ApplicationServices;
        var appSettings = serviceProvider.GetRequiredService<AppSettings>();
        if (appSettings.Infrastructure.RateLimiting.Enabled)
        {
            app.UseRateLimiter();
        }

        // Health checks
        app.UseHealthChecks("/health");
        app.UseHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        return app;
    }
}
