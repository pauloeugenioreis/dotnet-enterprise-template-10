using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Application.Services;
using ProjectTemplate.Data.Repository;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Dependency injection extensions for repositories and services
/// Uses Scrutor for automatic assembly scanning and registration
/// </summary>
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        // Scan and register repositories
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(Repository<>))
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        // Scan and register services
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(Service<>))
            .AddClasses(classes => classes.AssignableTo(typeof(IService<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        // Register generic repository and service (fallback)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IService<>), typeof(Service<>));

        return services;
    }
}
