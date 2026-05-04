using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectTemplate.Domain;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Extension methods for Redis distributed cache configuration
/// </summary>
public static class RedisExtension
{
    public static IServiceCollection AddRedis(this IServiceCollection services, AppSettings appSettings)
    {
        var connectionString = appSettings.Infrastructure?.Redis?.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDistributedMemoryCache();
            return services;
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = "ProjectTemplate_";
        });

        return services;
    }
}
