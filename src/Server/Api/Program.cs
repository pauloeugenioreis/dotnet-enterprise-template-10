using Npgsql;
using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Data.Context;
using ProjectTemplate.Data.Seeders;
using ProjectTemplate.Infrastructure.Extensions;
using ProjectTemplate.Infrastructure.Filters;
using ProjectTemplate.Domain.Interfaces;
using Polly;
using Polly.Retry;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// Configure CORS for multi-platform UIs
builder.Services.AddCors(options =>
{
    options.AddPolicy("EnterpriseCorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});


// Add Swagger with JWT authentication (skip in Testing environment to avoid version conflicts)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSwaggerWithAuth();
}

// Add infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

// ============================================================
// OPTIONAL FEATURES — Uncomment to enable
// See docs/FEATURES.md for configuration details
// ============================================================

// Add Custom Logging (structured JSON + Google Cloud Logging)
// builder.AddCustomLogging();

// Add MongoDB (NoSQL document store)
// builder.Services.AddMongo<Program>();

// Add Quartz.NET (background job scheduler)
// builder.Services.AddCustomizedQuartz((q, settings) =>
// {
//     // Example: Daily cleanup at 3 AM
//     // var cleanupJobKey = new JobKey("cleanup-job");
//     // q.AddJob<CleanupJob>(opts => opts.WithIdentity(cleanupJobKey));
//     // q.AddTrigger(opts => opts
//     //     .ForJob(cleanupJobKey)
//     //     .WithIdentity("cleanup-trigger")
//     //     .WithCronSchedule("0 0 3 * * ?"));
// });

// Add RabbitMQ (message queue / event-driven)
// builder.Services.AddRabbitMq();

// Add Cloud Storage (Google Cloud Storage / Azure Blob / AWS S3)
// builder.Services.AddStorage<Program>();

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

// Seed database on startup (Development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Use Polly to wait for database readiness (useful for Docker containers like Oracle/SQL Server)
    var retryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<Exception>(ex => 
                // Don't retry if the error is "Object already exists" (ORA-00955)
                !ex.Message.Contains("ORA-00955")),
            Delay = TimeSpan.FromSeconds(5),
            MaxRetryAttempts = 30,
            BackoffType = DelayBackoffType.Constant,
            OnRetry = args =>
            {
                Console.WriteLine($"⚠️ Database not ready yet. Retrying in 5s... (Attempt {args.AttemptNumber}/30)");
                Console.WriteLine($"   Reason: {args.Outcome.Exception?.Message}");
                return default;
            }
        })
        .Build();

    await retryPipeline.ExecuteAsync(async token =>
    {
        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync(token);
        }
        catch (Exception ex) when (ex.Message.Contains("ORA-00955"))
        {
            // Ignore "Table already exists" error in Oracle
            Console.WriteLine("ℹ️ Database objects already exist. Skipping creation.");
        }

        // Run seeder
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

        try 
        {
            // Get AppSettings from DI
            var appSettings = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ProjectTemplate.Domain.AppSettings>>().Value;

            // Ensure PostgreSQL database for Marten exists
            var pgConnString = appSettings.Infrastructure.EventSourcing.ConnectionString;
            if (!string.IsNullOrEmpty(pgConnString))
            {
                var builder = new Npgsql.NpgsqlConnectionStringBuilder(pgConnString);
                var targetDb = builder.Database;
                builder.Database = "postgres"; // Connect to admin DB
                
                using var conn = new Npgsql.NpgsqlConnection(builder.ConnectionString);
                await conn.OpenAsync(token);
                
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{targetDb}'";
                var exists = await cmd.ExecuteScalarAsync(token) != null;
                
                if (!exists)
                {
                    Console.WriteLine($"ℹ️ Creating PostgreSQL database '{targetDb}'...");
                    cmd.CommandText = $"CREATE DATABASE \"{targetDb}\"";
                    await cmd.ExecuteNonQueryAsync(token);
                }
            }

            // Warm up Marten / Ensure Schema is ready before seeding
            await eventStore.GetStatisticsAsync();
        }
        catch (Exception ex)
        {
             Console.WriteLine($"ℹ️ Event Store (Marten) not fully ready yet: {ex.Message}");
             throw; // Rethrow to trigger Polly retry if connection fails
        }

        var seeder = new DbSeeder(context, eventStore);
        await seeder.SeedAsync();
    });

    // Run MongoDB seeder (optional - enabled by scripts when MongoDB is selected)
    // await MongoDbSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithAuth();
}
else if (app.Environment.IsEnvironment("Testing"))
{
    // Skip Swagger in Testing environment
}

app.UseHttpsRedirection();

// Security Headers
app.UseHsts();
app.UseXContentTypeOptions();
app.UseReferrerPolicy(opts => opts.NoReferrer());
app.UseXXssProtection(opts => opts.EnabledWithBlockMode());
app.UseXfo(opts => opts.Deny());


// Use infrastructure middleware
app.UseInfrastructureMiddleware();

// Prometheus metrics endpoint (if telemetry is enabled)
var telemetryEnabled = app.Configuration.GetValue<bool>("AppSettings:Infrastructure:Telemetry:Enabled");
var telemetryProviders = app.Configuration.GetSection("AppSettings:Infrastructure:Telemetry:Providers").Get<string[]>() ?? Array.Empty<string>();
if (telemetryEnabled && telemetryProviders.Contains("prometheus", StringComparer.OrdinalIgnoreCase))
{
    app.MapPrometheusScrapingEndpoint();
}

app.UseCors("EnterpriseCorsPolicy");
app.UseAuthorization();

app.MapControllers();

await app.RunAsync().ConfigureAwait(false);

// Make Program class accessible for testing
public partial class Program { }
