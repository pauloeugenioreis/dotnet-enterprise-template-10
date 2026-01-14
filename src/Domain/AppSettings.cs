namespace ProjectTemplate.Domain;

/// <summary>
/// Application settings configuration
/// </summary>
public class AppSettings
{
    public string EnvironmentName { get; set; } = string.Empty;
    public ConnectionStringSettings ConnectionStrings { get; set; } = new();
    public InfrastructureSettings Infrastructure { get; set; } = new();
    public AuthenticationSettings Authentication { get; set; } = new();

    public bool IsDevelopment() => EnvironmentName == "Development";
    public bool IsTesting() => EnvironmentName == "Testing";
    public bool IsStaging() => EnvironmentName == "Staging";
    public bool IsProduction() => EnvironmentName == "Production";
}

public class ConnectionStringSettings
{
    public string DefaultConnection { get; set; } = string.Empty;
    public string ReadOnlyConnection { get; set; } = string.Empty;
    public string? MongoDB { get; set; }
    public string? RabbitMQ { get; set; }
    public string? ServiceAccount { get; set; } // Google Cloud Service Account JSON
}

public class InfrastructureSettings
{
    public string Environment { get; set; } = "Development";
    public CacheSettings Cache { get; set; } = new();
    public DatabaseSettings Database { get; set; } = new();
    public QuartzSettings Quartz { get; set; } = new();
    public StorageSettings Storage { get; set; } = new();
    public TelemetrySettings Telemetry { get; set; } = new();
    public RateLimitingSettings RateLimiting { get; set; } = new();
}

public class CacheSettings
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "Memory"; // Memory, Redis, SqlServer
    public RedisSettings? Redis { get; set; }
    public int DefaultExpirationMinutes { get; set; } = 60;
}

public class RedisSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}

public class DatabaseSettings
{
    public string DatabaseType { get; set; } = "InMemory"; // InMemory, SqlServer, Oracle, PostgreSQL, MySQL
    public int CommandTimeoutSeconds { get; set; } = 30;
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public bool EnableDetailedErrors { get; set; } = false;
}

public class QuartzSettings
{
    public int MaxConcurrency { get; set; } = 10;
}

public class StorageSettings
{
    public string DefaultBucket { get; set; } = string.Empty;
}

public class AuthenticationSettings
{
    public JwtSettings? Jwt { get; set; }
}

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

public class TelemetrySettings
{
    public bool Enabled { get; set; } = false;
    public string[] Providers { get; set; } = Array.Empty<string>(); // console, jaeger, otlp, grafana, prometheus, applicationinsights, datadog, dynatrace
    public double SamplingRatio { get; set; } = 1.0; // 0.0 to 1.0 (1.0 = 100%)
    public bool EnableSqlInstrumentation { get; set; } = true;
    public bool EnableHttpInstrumentation { get; set; } = true;
    
    public JaegerSettings Jaeger { get; set; } = new();
    public ZipkinSettings Zipkin { get; set; } = new();
    public OtlpSettings Otlp { get; set; } = new();
    public ApplicationInsightsSettings ApplicationInsights { get; set; } = new();
    public DatadogSettings Datadog { get; set; } = new();
    public DynatraceSettings Dynatrace { get; set; } = new();
}

public class JaegerSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6831;
    public int MaxPayloadSizeInBytes { get; set; } = 4096;
}

public class ZipkinSettings
{
    public string Endpoint { get; set; } = "http://localhost:9411/api/v2/spans";
}

public class OtlpSettings
{
    public string Endpoint { get; set; } = "http://localhost:4317"; // gRPC endpoint
    public string Protocol { get; set; } = "grpc"; // grpc or http
    public string? Headers { get; set; } // Format: "key1=value1,key2=value2"
}

public class ApplicationInsightsSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableAdaptiveSampling { get; set; } = true;
    public bool EnableLiveMetrics { get; set; } = true;
}

public class DatadogSettings
{
    public string Endpoint { get; set; } = "http://localhost:4317"; // Datadog Agent OTLP endpoint
    public string ApiKey { get; set; } = string.Empty;
    public string Site { get; set; } = "datadoghq.com"; // or datadoghq.eu
    public string Environment { get; set; } = "development";
}

public class DynatraceSettings
{
    public string Endpoint { get; set; } = string.Empty; // https://{your-environment-id}.live.dynatrace.com/api/v2/otlp
    public string ApiToken { get; set; } = string.Empty;
    public string Environment { get; set; } = "development";
}

public class RateLimitingSettings
{
    public bool Enabled { get; set; } = false;
    public string DefaultPolicy { get; set; } = "fixed"; // fixed, sliding, token, concurrent, none
    public TimeSpan DefaultWindow { get; set; } = TimeSpan.FromMinutes(1);
    public string[] WhitelistedIps { get; set; } = Array.Empty<string>(); // IPs que não sofrem rate limiting
    public RateLimitingPolicies Policies { get; set; } = new();
}

public class RateLimitingPolicies
{
    public FixedWindowPolicy FixedWindow { get; set; } = new();
    public SlidingWindowPolicy SlidingWindow { get; set; } = new();
    public TokenBucketPolicy TokenBucket { get; set; } = new();
    public ConcurrencyPolicy Concurrency { get; set; } = new();
}

public class FixedWindowPolicy
{
    public bool Enabled { get; set; } = true;
    public int PermitLimit { get; set; } = 100; // Número de requests permitidas
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1); // Janela de tempo
    public int QueueLimit { get; set; } = 10; // Requests enfileiradas quando limite atingido
}

public class SlidingWindowPolicy
{
    public bool Enabled { get; set; } = true;
    public int PermitLimit { get; set; } = 100;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
    public int SegmentsPerWindow { get; set; } = 6; // Divide a janela em 6 segmentos de 10s cada
    public int QueueLimit { get; set; } = 10;
}

public class TokenBucketPolicy
{
    public bool Enabled { get; set; } = true;
    public int TokenLimit { get; set; } = 100; // Capacidade máxima do balde
    public TimeSpan ReplenishmentPeriod { get; set; } = TimeSpan.FromSeconds(10); // A cada quanto tempo reabastece
    public int TokensPerPeriod { get; set; } = 10; // Quantos tokens adiciona por período
    public bool AutoReplenishment { get; set; } = true;
    public int QueueLimit { get; set; } = 10;
}

public class ConcurrencyPolicy
{
    public bool Enabled { get; set; } = true;
    public int PermitLimit { get; set; } = 50; // Requests simultâneas permitidas
    public int QueueLimit { get; set; } = 100; // Requests enfileiradas
}

