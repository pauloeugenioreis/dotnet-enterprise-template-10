using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using ProjectTemplate.Domain.Interfaces;
using ProjectTemplate.Data.Repository;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Dependency injection configuration using Scrutor for automatic registration
/// </summary>
public static class DependencyInjectionExtension
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        // Register repositories using Scrutor
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(Repository<>))
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Register services using Scrutor
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(IService<>))
            .AddClasses(classes => classes.AssignableTo(typeof(IService<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Register infrastructure services
        services.Scan(scan => scan
            .FromAssemblyOf<InfrastructureExtensions>()
            .AddClasses(classes => classes.Where(type =>
                type.Name.EndsWith("Service") &&
                !type.Name.Equals("Service") &&
                type.Namespace?.Contains("Infrastructure.Services") == true))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
