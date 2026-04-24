using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectTemplate.Domain;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Extension methods for Redis distributed cache configuration
/// </summary>
public static class RedisExtension
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IOptions<AppSettings> appSettings)
    {
        var connectionString = appSettings.Value.Infrastructure?.Redis?.ConnectionString;

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
