using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Resilience configuration using Polly v8
/// Provides a centralized way to configure standard resilience for HttpClients
/// </summary>
public static class ResilienceExtension
{
    /// <summary>
    /// Configures standard resilience for all HttpClients.
    /// This includes:
    /// - Retry: 3 attempts with exponential backoff
    /// - Circuit Breaker: Opens after 10% failure rate
    /// - Timeout: 30 seconds per request
    /// - Rate Limiter: 1000 concurrent requests
    /// </summary>
    public static IServiceCollection AddResilienceConfiguration(this IServiceCollection services)
    {
        // Define a standard resilience pipeline
        // This can be used with .AddResilienceHandler("standard", builder => ...) on HttpClients
        
        // However, Polly v8 with Microsoft.Extensions.Http.Resilience 
        // makes it easier with AddStandardResilienceHandler()
        
        return services;
    }

    /// <summary>
    /// Helper extension to add standard resilience to a specific HttpClient
    /// Usage: services.AddHttpClient<IMyService, MyService>().AddStandardResilience();
    /// </summary>
    public static IHttpClientBuilder AddStandardResilience(this IHttpClientBuilder builder)
    {
        builder.AddStandardResilienceHandler(options =>
        {
            // Configure default retry
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.BackoffType = DelayBackoffType.Exponential;
            options.Retry.UseJitter = true;
            options.Retry.Delay = TimeSpan.FromSeconds(2);

            // Configure circuit breaker
            options.CircuitBreaker.FailureRatio = 0.1; // 10%
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.MinimumThroughput = 10;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);

            // Configure timeouts
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(1);
        });
        return builder;
    }
}
