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
                if (string.IsNullOrEmpty(cacheSettings.ConnectionString))
                {
                    throw new InvalidOperationException("Redis connection string is required when Redis provider is selected");
                }
                
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = cacheSettings.ConnectionString;
                    options.InstanceName = "ProjectTemplate_";
                });
                break;

            case "sqlserver":
                if (string.IsNullOrEmpty(cacheSettings.ConnectionString))
                {
                    throw new InvalidOperationException("SQL Server connection string is required when SqlServer provider is selected");
                }
                
                services.AddDistributedSqlServerCache(options =>
                {
                    options.ConnectionString = cacheSettings.ConnectionString;
                    options.SchemaName = "dbo";
                    options.TableName = "Cache";
                });
                break;

            case "memory":
            default:
                services.AddMemoryCache();
                services.AddDistributedMemoryCache();
                break;
        }

        return services;
    }
}
