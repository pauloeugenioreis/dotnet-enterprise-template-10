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
        // Scan and register ALL repositories automatically using AsMatchingInterface()
        // AsMatchingInterface() registers only the interface that matches the class name:
        // 
        // ✅ Repository<Product> → IRepository<Product>
        // ✅ ProductDapperRepository → IProductDapperRepository (NOT IRepository<Product>)
        // ✅ ProductAdoRepository → IProductAdoRepository (NOT IRepository<Product>)
        // ✅ OrderDapperRepository → IOrderDapperRepository (NOT IRepository<Order>)
        // ✅ OrderAdoRepository → IOrderAdoRepository (NOT IRepository<Order>)
        //
        // This prevents conflicts: alternative ORMs won't overwrite base IRepository<T>
        // Tests use IRepository<T> → EF Core InMemory ✅
        // Production can inject specific ORMs: IProductDapperRepository → ProductDapperRepository ✅
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(Repository<>))
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
            .AsMatchingInterface()
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

        // Register UserRepository explicitly (doesn't inherit from Repository<T>)
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
