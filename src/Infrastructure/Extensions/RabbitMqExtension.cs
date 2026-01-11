using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Domain.Interfaces;
using ProjectTemplate.Infrastructure.Services;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Extension methods for RabbitMQ message queue configuration
/// Provides message broker capabilities for async communication
/// </summary>
public static class RabbitMqExtension
{
    /// <summary>
    /// Adds RabbitMQ services to the DI container
    /// Registers queue service for publishing messages
    /// </summary>
    public static IServiceCollection AddRabbitMq(this IServiceCollection services)
    {
        services.AddScoped<IQueueService, QueueService>();
        return services;
    }
}
