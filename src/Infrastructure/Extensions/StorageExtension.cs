using Google.Cloud.Storage.V1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Interfaces;
using ProjectTemplate.Infrastructure.Services;
using System;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Extension methods for Google Cloud Storage configuration
/// Provides blob storage capabilities for file upload/download
/// </summary>
public static class StorageExtension
{
    /// <summary>
    /// Adds Google Cloud Storage services to the DI container
    /// Configures StorageClient with authentication
    /// </summary>
    public static IServiceCollection AddStorage<TProgram>(this IServiceCollection services)
    {
        services.AddSingleton(sp => CreateStorageClient<TProgram>(sp));
        services.AddScoped<IStorageService, StorageService>();
        return services;
    }

    private static StorageClient CreateStorageClient<TProgram>(IServiceProvider serviceProvider)
    {
        var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        var logger = serviceProvider.GetRequiredService<ILogger<TProgram>>();
        var isProduction = appSettings.Infrastructure?.Environment == "Production";

        // Try to create from explicit service account JSON
        if (!string.IsNullOrEmpty(appSettings.Infrastructure?.Storage?.ServiceAccount))
        {
            try
            {
                var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromJson(
                    appSettings.Infrastructure.Storage.ServiceAccount);
                logger.LogInformation("StorageClient created using explicit Service Account credentials.");
                return StorageClient.Create(credential);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to parse Service Account JSON. Using default credentials as fallback.");
            }
        }

        // Try to create from default environment credentials
        try
        {
            logger.LogInformation("Creating StorageClient with default environment credentials.");
            return StorageClient.Create();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to create StorageClient. No valid Google Cloud credentials found.");

            if (isProduction)
            {
                throw;
            }

            logger.LogWarning("Returning unauthenticated StorageClient for non-production environment.");
            return StorageClient.CreateUnauthenticated();
        }
    }
}
