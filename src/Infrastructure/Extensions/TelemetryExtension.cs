using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProjectTemplate.Domain;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Telemetry and observability configuration using OpenTelemetry
/// Supports multiple backends: Jaeger, Grafana, Prometheus, Datadog, Dynatrace, Application Insights
/// </summary>
public static class TelemetryExtension
{
    public static IServiceCollection AddTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var settings = configuration.GetSection("AppSettings:Infrastructure:Telemetry").Get<TelemetrySettings>();

        if (settings?.Enabled != true)
        {
            Console.WriteLine("⚠️  Telemetry is disabled");
            return services;
        }

        var serviceName = configuration["AppSettings:Application:Name"] ?? "ProjectTemplate.Api";
        var serviceVersion = configuration["AppSettings:Application:Version"] ?? "1.0.0";

        // Resource attributes (identificação do serviço)
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: serviceName,
                serviceVersion: serviceVersion,
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = environment.EnvironmentName,
                ["host.name"] = Environment.MachineName,
                ["telemetry.sdk.name"] = "opentelemetry",
                ["telemetry.sdk.language"] = "dotnet",
                ["telemetry.sdk.version"] = "1.7.0"
            });

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName, serviceVersion))
            .WithTracing(tracerProvider =>
            {
                // Instrumentação automática
                tracerProvider
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, httpRequest) =>
                        {
                            activity.SetTag("http.request.method", httpRequest.Method);
                            activity.SetTag("http.request.path", httpRequest.Path);
                            activity.SetTag("http.request.query", httpRequest.QueryString.ToString());
                            activity.SetTag("http.request.user_agent", httpRequest.Headers.UserAgent.ToString());
                        };
                        options.EnrichWithHttpResponse = (activity, httpResponse) =>
                        {
                            activity.SetTag("http.response.status_code", httpResponse.StatusCode);
                        };
                        options.Filter = (httpContext) =>
                        {
                            // Não rastrear health checks para reduzir ruído
                            return !httpContext.Request.Path.StartsWithSegments("/health");
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                        {
                            activity.SetTag("http.client.method", httpRequestMessage.Method.ToString());
                            activity.SetTag("http.client.url", httpRequestMessage.RequestUri?.ToString());
                        };
                    });

                if (settings.EnableSqlInstrumentation)
                {
                    tracerProvider.AddSqlClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    });
                }

                tracerProvider
                    .AddEntityFrameworkCoreInstrumentation();

                // Sampling (reduz volume em produção)
                if (settings.SamplingRatio > 0 && settings.SamplingRatio < 1.0)
                {
                    tracerProvider.SetSampler(new TraceIdRatioBasedSampler(settings.SamplingRatio));
                }

                // Configurar exportadores baseado no provider
                ConfigureTraceExporters(tracerProvider, settings, environment);
            })
            .WithMetrics(meterProvider =>
            {
                meterProvider
                    .SetResourceBuilder(resourceBuilder)
                    .AddMeter(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                // Configurar exportadores de métricas
                ConfigureMetricExporters(meterProvider, settings, environment);
            });

        Console.WriteLine($"✅ Telemetry enabled: {string.Join(", ", settings.Providers)}");
        return services;
    }

    private static void ConfigureTraceExporters(
        TracerProviderBuilder builder,
        TelemetrySettings settings,
        IHostEnvironment environment)
    {
        foreach (var provider in settings.Providers)
        {
            switch (provider.ToLowerInvariant())
            {
                case "console":
                    builder.AddConsoleExporter();
                    Console.WriteLine("  📊 Console exporter enabled");
                    break;

                case "jaeger":
                    builder.AddJaegerExporter(options =>
                    {
                        options.AgentHost = settings.Jaeger.Host;
                        options.AgentPort = settings.Jaeger.Port;
                        options.MaxPayloadSizeInBytes = settings.Jaeger.MaxPayloadSizeInBytes;
                    });
                    Console.WriteLine($"  📊 Jaeger exporter enabled: {settings.Jaeger.Host}:{settings.Jaeger.Port}");
                    break;

                case "zipkin":
                    builder.AddZipkinExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Zipkin.Endpoint);
                    });
                    Console.WriteLine($"  📊 Zipkin exporter enabled: {settings.Zipkin.Endpoint}");
                    break;

                case "otlp":
                case "grafana":
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Otlp.Endpoint);
                        options.Protocol = settings.Otlp.Protocol == "grpc" 
                            ? OtlpExportProtocol.Grpc 
                            : OtlpExportProtocol.HttpProtobuf;
                        
                        if (!string.IsNullOrEmpty(settings.Otlp.Headers))
                        {
                            options.Headers = settings.Otlp.Headers;
                        }
                    });
                    Console.WriteLine($"  📊 OTLP/Grafana exporter enabled: {settings.Otlp.Endpoint}");
                    break;

                case "applicationinsights":
                case "azure":
                    // TODO: Application Insights requires Azure.Monitor.OpenTelemetry.AspNetCore package
                    // builder.AddAzureMonitorTraceExporter(options =>
                    // {
                    //     options.ConnectionString = settings.ApplicationInsights.ConnectionString;
                    // });
                    Console.WriteLine($"  ⚠️  Application Insights not yet configured (requires Azure.Monitor.OpenTelemetry.AspNetCore)");
                    break;

                case "datadog":
                    // Datadog usa OTLP protocol
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Datadog.Endpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                        options.Headers = $"DD-API-KEY={settings.Datadog.ApiKey}";
                    });
                    Console.WriteLine($"  📊 Datadog exporter enabled: {settings.Datadog.Endpoint}");
                    break;

                case "dynatrace":
                    // Dynatrace usa OTLP protocol
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Dynatrace.Endpoint);
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                        options.Headers = $"Authorization=Api-Token {settings.Dynatrace.ApiToken}";
                    });
                    Console.WriteLine($"  📊 Dynatrace exporter enabled: {settings.Dynatrace.Endpoint}");
                    break;

                default:
                    Console.WriteLine($"  ⚠️  Unknown telemetry provider: {provider}");
                    break;
            }
        }

        // Console exporter sempre ativo em Development (para debug)
        if (environment.IsDevelopment() && !settings.Providers.Contains("console", StringComparer.OrdinalIgnoreCase))
        {
            builder.AddConsoleExporter();
            Console.WriteLine("  📊 Console exporter enabled (Development)");
        }
    }

    private static void ConfigureMetricExporters(
        MeterProviderBuilder builder,
        TelemetrySettings settings,
        IHostEnvironment environment)
    {
        foreach (var provider in settings.Providers)
        {
            switch (provider.ToLowerInvariant())
            {
                case "console":
                    builder.AddConsoleExporter();
                    break;

                case "prometheus":
                    // Prometheus usa scraping, não push
                    // Endpoint será exposto em /metrics
                    builder.AddPrometheusExporter();
                    Console.WriteLine("  📈 Prometheus exporter enabled (endpoint: /metrics)");
                    break;

                case "otlp":
                case "grafana":
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Otlp.Endpoint);
                        options.Protocol = settings.Otlp.Protocol == "grpc" 
                            ? OtlpExportProtocol.Grpc 
                            : OtlpExportProtocol.HttpProtobuf;
                        
                        if (!string.IsNullOrEmpty(settings.Otlp.Headers))
                        {
                            options.Headers = settings.Otlp.Headers;
                        }
                    });
                    break;

                case "applicationinsights":
                case "azure":
                    // TODO: Application Insights requires Azure.Monitor.OpenTelemetry.AspNetCore package
                    // builder.AddAzureMonitorMetricExporter(options =>
                    // {
                    //     options.ConnectionString = settings.ApplicationInsights.ConnectionString;
                    // });
                    break;

                case "datadog":
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Datadog.Endpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                        options.Headers = $"DD-API-KEY={settings.Datadog.ApiKey}";
                    });
                    break;

                case "dynatrace":
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(settings.Dynatrace.Endpoint);
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                        options.Headers = $"Authorization=Api-Token {settings.Dynatrace.ApiToken}";
                    });
                    break;
            }
        }
    }
}
