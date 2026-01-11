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
            switch (settings.DatabaseType.ToLower())
            {
                case "sqlserver":
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.CommandTimeout(settings.CommandTimeout);
                        sqlOptions.EnableRetryOnFailure(3);
                    });
                    break;

                case "oracle":
                    options.UseOracle(connectionString, oracleOptions =>
                    {
                        oracleOptions.CommandTimeout(settings.CommandTimeout);
                    });
                    break;

                case "postgresql":
                case "postgres":
                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.CommandTimeout(settings.CommandTimeout);
                        npgsqlOptions.EnableRetryOnFailure(3);
                    });
                    break;

                case "mysql":
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions =>
                    {
                        mySqlOptions.CommandTimeout(settings.CommandTimeout);
                        mySqlOptions.EnableRetryOnFailure(3);
                    });
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported database type: {settings.DatabaseType}");
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
