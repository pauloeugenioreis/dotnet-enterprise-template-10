using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using ProjectTemplate.Domain;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Health checks configuration
/// </summary>
public static class HealthChecksExtension
{
    public static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        var healthChecksBuilder = services.AddHealthChecks();

        // Application self-check
        healthChecksBuilder.AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "ready" });

        // Database health check
        var connectionString = appSettings?.ConnectionStrings?.DefaultConnection;
        if (!string.IsNullOrEmpty(connectionString))
        {
            var dbType = appSettings?.Infrastructure?.Database?.DatabaseType?.ToLower() ?? "sqlserver";
            
            switch (dbType)
            {
                case "sqlserver":
                    healthChecksBuilder.AddSqlServer(
                        connectionString,
                        name: "database",
                        failureStatus: HealthStatus.Degraded,
                        tags: new[] { "ready", "database" });
                    break;

                case "oracle":
                    healthChecksBuilder.AddOracle(
                        connectionString,
                        name: "database",
                        failureStatus: HealthStatus.Degraded,
                        tags: new[] { "ready", "database" });
                    break;

                case "postgresql":
                case "postgres":
                    healthChecksBuilder.AddNpgSql(
                        connectionString,
                        name: "database",
                        failureStatus: HealthStatus.Degraded,
                        tags: new[] { "ready", "database" });
                    break;

                // Add other database types as needed
            }
        }

        // Add Redis health check if configured
        var cacheSettings = appSettings?.Infrastructure?.Cache;
        if (cacheSettings?.Enabled == true && 
            cacheSettings.Provider.Equals("Redis", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrEmpty(cacheSettings.ConnectionString))
        {
            healthChecksBuilder.AddRedis(
                cacheSettings.ConnectionString,
                name: "redis",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready", "cache" });
        }

        return services;
    }
}
