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
    public string Provider { get; set; } = "EntityFrameworkCore"; // EntityFrameworkCore, Dapper, NHibernate, Linq2Db
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
