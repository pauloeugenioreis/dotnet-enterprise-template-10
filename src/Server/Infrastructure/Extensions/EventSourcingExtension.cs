using Marten;
using Npgsql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;
using ProjectTemplate.Infrastructure.Services;
using Weasel.Core;
using Microsoft.Extensions.Configuration;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Event Sourcing infrastructure configuration
/// </summary>
public static class EventSourcingExtension
{
    /// <summary>
    /// Add Event Sourcing services to the DI container
    /// </summary>
    public static IServiceCollection AddEventSourcing(this IServiceCollection services, IOptions<AppSettings> appSettings)
    {
        var settings = appSettings.Value.Infrastructure.EventSourcing;

        // Register settings so repositories/controllers can resolve even when disabled
        services.AddSingleton(settings);

        if (!settings.Enabled)
        {
            // Event Sourcing disabled - register empty implementation
            services.AddScoped<IEventStore, NoOpEventStore>();
            return services;
        }

        // Configure based on provider
        switch (settings.Provider.ToLowerInvariant())
        {
            case "marten":
                ConfigureMarten(services, settings);
                break;

            case "custom":
                // For future custom implementations
                services.AddScoped<IEventStore, NoOpEventStore>();
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported Event Sourcing provider: {settings.Provider}");
        }

        return services;
    }

    private static void ConfigureMarten(IServiceCollection services, EventSourcingSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new InvalidOperationException(
                "EventSourcing:ConnectionString is required when using Marten provider");
        }

        // Configure Marten
        services.AddMarten(sp =>
        {
            var options = new StoreOptions();
            options.Connection(settings.ConnectionString);

            // Configure Marten to create the database if it doesn't exist
            options.CreateDatabasesForTenants(c =>
            {
                var builder = new Npgsql.NpgsqlConnectionStringBuilder(settings.ConnectionString);
                builder.Database = "postgres";
                c.MaintenanceDatabase(builder.ConnectionString);
            });

            // Set AutoCreateSchemaObjects to All via reflection to avoid namespace issues across Marten versions
            var autoCreateProperty = options.GetType().GetProperty(nameof(options.AutoCreateSchemaObjects));
            if (autoCreateProperty != null)
            {
                var allValue = Enum.Parse(autoCreateProperty.PropertyType, "All");
                autoCreateProperty.SetValue(options, allValue);
            }

            // Set StreamIdentity to AsString using reflection to handle Marten 7/8+ variations
            var streamIdentityProperty = options.Events.GetType().GetProperty(nameof(options.Events.StreamIdentity));
            if (streamIdentityProperty?.PropertyType.IsEnum == true)
            {
                var streamIdentityValue = Enum.Parse(streamIdentityProperty.PropertyType, "AsString");
                streamIdentityProperty.SetValue(options.Events, streamIdentityValue);
            }

            return options;
        });

        // Register Event Store implementation
        services.AddScoped<IEventStore, MartenEventStore>();
    }
}
