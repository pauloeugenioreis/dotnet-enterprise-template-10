using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Domain;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Cache configuration supporting multiple providers
/// </summary>
public static class CacheExtension
{
    public static IServiceCollection AddCacheConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        var cacheSettings = appSettings?.Infrastructure?.Cache ?? new CacheSettings();

        if (!cacheSettings.Enabled)
        {
            // Use memory cache as fallback
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            return services;
        }

        switch (cacheSettings.Provider.ToLower())
        {
            case "redis":
                var redisConnectionString = cacheSettings.Redis?.ConnectionString;
                if (string.IsNullOrEmpty(redisConnectionString))
                {
                    throw new InvalidOperationException("Redis connection string is required when Redis provider is selected");
                }
                
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "ProjectTemplate_";
                });
                break;

            case "sqlserver":
                // SQL Server distributed cache requires:
                // Install-Package Microsoft.Extensions.Caching.SqlServer
                // dotnet sql-cache create "connection-string" dbo Cache
                throw new NotImplementedException("SQL Server cache requires Microsoft.Extensions.Caching.SqlServer package");

            case "memory":
            default:
                services.AddMemoryCache();
                services.AddDistributedMemoryCache();
                break;
        }

        return services;
    }
}
