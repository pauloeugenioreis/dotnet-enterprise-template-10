using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Application;
using ProjectTemplate.Application.Services;
using ProjectTemplate.Data;
using ProjectTemplate.Data.Repository;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Interfaces;
using ProjectTemplate.Infrastructure;
using ProjectTemplate.Infrastructure.Services;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Dependency injection extensions for repositories and services
/// Uses Scrutor for automatic assembly scanning and registration
/// </summary>
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, AppSettings appSettings)
    {
        // Scan and register ALL repositories automatically using AsMatchingInterface()
        // AsMatchingInterface() registers only the interface that matches the class name:
        //
        // ✅ ProductRepository → IProductRepository
        // ✅ OrderRepository → IOrderRepository
        // ✅ ProductDapperRepository → IProductDapperRepository (NOT IRepository<Product>)
        // Scan and register repositories
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(DataAssemblyMarker))
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
            .AsMatchingInterface()
            .WithScopedLifetime()
        );

        // Scan and register services
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(ApplicationAssemblyMarker))
            .AddClasses(classes => classes.AssignableTo(typeof(IService<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        // Register generic repository and service (fallback)
        if (appSettings.Infrastructure.EventSourcing.Enabled)
        {
            // Register event payload factories
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(InfrastructureAssemblyMarker))
                .AddClasses(classes => classes.AssignableTo(typeof(IEventPayloadFactory<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            );
            services.AddScoped(typeof(IRepository<>), typeof(HybridRepository<>));
        }
        else
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        }
        services.AddScoped(typeof(IService<>), typeof(Service<>));

        // Register UserRepository explicitly (doesn't inherit from Repository<T>)
        services.AddScoped<IUserRepository, UserRepository>();

        // Register execution context service (provides user/metadata without HTTP coupling)
        services.AddScoped<IExecutionContextService, ExecutionContextService>();

        // Register ALL FluentValidation validators from the Domain assembly.
        // DomainAssemblyMarker is used as an anchor — it does NOT limit scanning to a single class.
        // Every AbstractValidator<T> found in the Domain project will be registered automatically.
        services.AddValidatorsFromAssemblyContaining<DomainAssemblyMarker>();

        return services;
    }
}
