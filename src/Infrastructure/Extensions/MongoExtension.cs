using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProjectTemplate.Domain;
using System;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Extension methods for MongoDB configuration
/// Provides MongoDB client and database setup with proper error handling
/// </summary>
public static class MongoExtension
{
    /// <summary>
    /// Adds MongoDB services to the DI container
    /// Configures MongoClient and default database
    /// </summary>
    public static IServiceCollection AddMongo<TProgram>(this IServiceCollection services)
    {
        services.AddSingleton<IMongoClient>(sp => CreateMongoClient<TProgram>(
            sp.GetRequiredService<IOptions<AppSettings>>(), 
            sp.GetRequiredService<ILogger<TProgram>>()));

        // Register default IMongoDatabase resolved from connection string
        services.AddSingleton(sp =>
        {
            var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var connectionString = appSettings.ConnectionStrings?.MongoDB;
            
            var mongoUrl = new MongoUrl(string.IsNullOrWhiteSpace(connectionString) 
                ? "mongodb://null-mongodb-for-development:27017" 
                : connectionString);
                
            var databaseName = string.IsNullOrWhiteSpace(mongoUrl.DatabaseName) 
                ? "projecttemplate" 
                : mongoUrl.DatabaseName;
                
            return client.GetDatabase(databaseName);
        });

        // Register your MongoDB-based services here
        // Example: services.AddScoped<IYourMongoService, YourMongoService>();

        return services;
    }

    private static IMongoClient CreateMongoClient<TProgram>(
        IOptions<AppSettings> settings, 
        ILogger<TProgram> logger)
    {
        var appSettings = settings.Value;
        var isProduction = appSettings.Infrastructure?.Environment == "Production";

        try
        {
            var connectionString = appSettings.ConnectionStrings?.MongoDB;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                logger.LogWarning("MongoDB connection string is empty.");

                if (isProduction)
                {
                    throw new InvalidOperationException("MongoDB connection string cannot be empty in production.");
                }

                logger.LogWarning("Using null MongoDB client for development.");
                return new MongoClient("mongodb://null-mongodb-for-development:27017");
            }

            var mongoUrl = new MongoUrl(connectionString);
            var commandTimeoutSeconds = appSettings.Infrastructure?.Database?.CommandTimeoutSeconds ?? 300;

            var mongoSettings = MongoClientSettings.FromUrl(mongoUrl);
            mongoSettings.RetryWrites = true;
            mongoSettings.SocketTimeout = TimeSpan.FromSeconds(commandTimeoutSeconds);
            mongoSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(Math.Min(30, Math.Max(2, commandTimeoutSeconds / 10)));
            mongoSettings.ConnectTimeout = TimeSpan.FromSeconds(Math.Min(10, Math.Max(2, commandTimeoutSeconds / 30)));

            logger.LogInformation("Creating MongoClient for servers: {Servers}", string.Join(',', mongoSettings.Servers));

            return new MongoClient(mongoSettings);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating MongoClient.");

            if (isProduction)
            {
                throw;
            }

            logger.LogWarning("Returning fallback MongoClient for development.");
            return new MongoClient("mongodb://null-mongodb-for-development:27017");
        }
    }
}
