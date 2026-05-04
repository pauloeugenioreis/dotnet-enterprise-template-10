using System;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Domain;
using Quartz;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Extension methods for Quartz.NET (Background Jobs/Scheduled Tasks)
/// Provides job scheduling capabilities with in-memory store
/// </summary>
public static class QuartzExtension
{
    /// <summary>
    /// Adds Quartz.NET services to the DI container
    /// Configures scheduler with custom settings
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="appSettings">Application settings (already bound)</param>
    /// <param name="configureJobs">Optional action to configure jobs</param>
    public static IServiceCollection AddCustomizedQuartz(
        this IServiceCollection services,
        AppSettings appSettings,
        Action<IServiceCollectionQuartzConfigurator, AppSettings>? configureJobs = null)
    {
        ArgumentNullException.ThrowIfNull(appSettings);

        var maxConcurrency = appSettings.Infrastructure?.Quartz?.MaxConcurrency ?? 10;

        services.AddQuartz(q =>
        {
            q.SchedulerName = "Quartz Scheduler";
            q.UseSimpleTypeLoader();
            q.UseInMemoryStore();
            q.UseDefaultThreadPool(tp => tp.MaxConcurrency = maxConcurrency);

            // Configure jobs if provided
            configureJobs?.Invoke(q, appSettings);
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}
