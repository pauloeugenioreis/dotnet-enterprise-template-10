using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Data.Context;
using ProjectTemplate.Domain;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Database configuration extension supporting multiple ORMs
/// </summary>
public static class DatabaseExtension
{
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        var dbSettings = appSettings?.Infrastructure?.Database ?? new DatabaseSettings();
        var connectionString = appSettings?.ConnectionStrings?.DefaultConnection 
            ?? throw new InvalidOperationException("DefaultConnection string is required");

        // Configure based on ORM provider
        switch (dbSettings.Provider.ToLower())
        {
            case "entityframework":
            case "ef":
            case "efcore":
                services.AddEntityFramework(connectionString, dbSettings);
                break;

            case "dapper":
                services.AddDapper(connectionString);
                break;

            case "nhibernate":
                // Implement NHibernate configuration
                throw new NotImplementedException("NHibernate configuration not implemented yet");

            case "linq2db":
                // Implement Linq2Db configuration
                throw new NotImplementedException("Linq2Db configuration not implemented yet");

            default:
                throw new InvalidOperationException($"Unsupported ORM provider: {dbSettings.Provider}");
        }

        return services;
    }

    private static IServiceCollection AddEntityFramework(
        this IServiceCollection services,
        string connectionString,
        DatabaseSettings settings)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // Configure database provider based on DatabaseType
            // For production, install and uncomment the appropriate provider:
            // - Microsoft.EntityFrameworkCore.SqlServer for SQL Server
            // - Oracle.EntityFrameworkCore for Oracle
            // - Npgsql.EntityFrameworkCore.PostgreSQL for PostgreSQL
            // - Pomelo.EntityFrameworkCore.MySql for MySQL
            
            switch (settings.DatabaseType.ToLower())
            {
                case "memory":
                case "inmemory":
                    options.UseInMemoryDatabase("ProjectTemplateDb");
                    break;

                // case "sqlserver":
                //     options.UseSqlServer(connectionString, sqlOptions =>
                //     {
                //         sqlOptions.CommandTimeout(settings.CommandTimeoutSeconds);
                //         sqlOptions.EnableRetryOnFailure(3);
                //     });
                //     break;

                default:
                    // Default to in-memory for testing/development
                    options.UseInMemoryDatabase("ProjectTemplateDb");
                    break;
            }

            // Common EF Core configurations
            if (settings.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            options.EnableDetailedErrors();
        });

        return services;
    }

    private static IServiceCollection AddDapper(
        this IServiceCollection services,
        string connectionString)
    {
        // Register connection string for Dapper repositories
        services.AddSingleton(connectionString);
        return services;
    }
}
