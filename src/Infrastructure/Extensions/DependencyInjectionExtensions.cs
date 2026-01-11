using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Application.Services;
using ProjectTemplate.Data.Repository;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Dependency injection extensions for repositories and services
/// </summary>
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        // Register generic repository and service
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IService<>), typeof(Service<>));

        // Register custom repositories
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Register custom services
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
