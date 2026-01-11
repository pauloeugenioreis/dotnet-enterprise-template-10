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
            
            // Database health checks require specific packages to be installed
            // Uncomment and install the appropriate package:
            // - AspNetCore.HealthChecks.SqlServer for SQL Server
            // - AspNetCore.HealthChecks.Oracle for Oracle
            // - AspNetCore.HealthChecks.Npgsql for PostgreSQL
            
            // switch (dbType)
            // {
            //     case "sqlserver":
            //         healthChecksBuilder.AddSqlServer(
            //             connectionString,
            //             name: "database",
            //             failureStatus: HealthStatus.Degraded,
            //             tags: new[] { "ready", "database" });
            //         break;
            // }
        }

        // Add Redis health check if configured
        // Requires: AspNetCore.HealthChecks.Redis package
        // var cacheSettings = appSettings?.Infrastructure?.Cache;
        // if (cacheSettings?.Enabled == true && 
        //     cacheSettings.Provider.Equals("Redis", StringComparison.OrdinalIgnoreCase) &&
        //     cacheSettings.Redis != null &&
        //     !string.IsNullOrEmpty(cacheSettings.Redis.ConnectionString))
        // {
        //     healthChecksBuilder.AddRedis(
        //         cacheSettings.Redis.ConnectionString,
        //         name: "redis",
        //         failureStatus: HealthStatus.Degraded,
        //         tags: new[] { "ready", "cache" });
        // }

        return services;
    }
}
