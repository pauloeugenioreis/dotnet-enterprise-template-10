using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
        // Bind, validate and register AppSettings.
        // - IOptions<AppSettings> pipeline (with on-start validation) for runtime consumers.
        // - Singleton AppSettings (POCO) for direct injection.
        // - Local POCO instance reused below to configure registration-time extensions
        //   without resorting to BuildServiceProvider (anti-pattern ASP0000).
        services.AddOptions<AppSettings>()
            .Bind(configuration.GetSection("AppSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>()
            ?? throw new InvalidOperationException(
                "Configuration section 'AppSettings' is missing or invalid.");

        services.AddSingleton<IOptions<AppSettings>>(Options.Create(appSettings));

        // Database
        services.AddDatabaseConfiguration(appSettings);

        // Health checks
        services.AddHealthChecksConfiguration(appSettings);

        // Telemetry (OpenTelemetry)
        services.AddTelemetry(appSettings);

        // Rate Limiting
        services.AddRateLimitingConfiguration(appSettings);

        // Event Sourcing
        services.AddEventSourcing(appSettings);

        // Resilience (Polly v8)
        services.AddResilienceConfiguration();

        // Distributed Cache (Redis)
        services.AddRedis(appSettings);

        // Authentication (JWT + OAuth2)
        services.AddJwtAuthentication(appSettings);

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

        // ProblemDetails service (RFC 7807) consumed by GlobalExceptionHandler
        services.AddProblemDetails();

        // API Versioning
        services.AddCustomizedApiVersioning();

        // Application dependencies
        services.AddApplicationDependencies(appSettings);

        return services;
    }

    public static IApplicationBuilder UseInfrastructureMiddleware(this IApplicationBuilder app)
    {
        // Global Exception Handler (RFC 7807 ProblemDetails)
        app.UseGlobalExceptionHandler();

        // CORS
        app.UseCors("DefaultCorsPolicy");

        // Response compression
        app.UseResponseCompression();

        // Authentication & Authorization
        app.UseJwtAuthentication();

        // Rate Limiting - Only if enabled in configuration
        var serviceProvider = app.ApplicationServices;
        var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
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
