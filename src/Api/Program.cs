using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Data.Context;
using ProjectTemplate.Data.Seeders;
using ProjectTemplate.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services
builder.Services.AddControllers();

// Add Output Cache (.NET 10)
builder.Services.AddOutputCache(options =>
{
    // Default policy: 10 seconds cache
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(10)));

    // Policy for GET endpoints: 5 minutes cache
    options.AddPolicy("Expire300", builder => builder.Expire(TimeSpan.FromSeconds(300)));

    // Policy for products: 10 minutes cache, vary by id
    options.AddPolicy("ProductCache", builder => builder
        .Expire(TimeSpan.FromMinutes(10))
        .SetVaryByQuery("id"));
});

// Add Swagger with JWT authentication (skip in Testing environment to avoid version conflicts)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSwaggerWithAuth();
}

// Add infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Seed database on startup (Development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Ensure database is created
    await context.Database.EnsureCreatedAsync();

    // Run seeder
    var seeder = new DbSeeder(context);
    await seeder.SeedAsync();
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

// Output Cache
app.UseOutputCache();

// Use infrastructure middleware
app.UseInfrastructureMiddleware(app.Configuration);

// Prometheus metrics endpoint (if telemetry is enabled)
var telemetryEnabled = app.Configuration.GetValue<bool>("AppSettings:Infrastructure:Telemetry:Enabled");
var telemetryProviders = app.Configuration.GetSection("AppSettings:Infrastructure:Telemetry:Providers").Get<string[]>() ?? Array.Empty<string>();
if (telemetryEnabled && telemetryProviders.Contains("prometheus", StringComparer.OrdinalIgnoreCase))
{
    app.MapPrometheusScrapingEndpoint();
}

app.UseAuthorization();

app.MapControllers();

await app.RunAsync().ConfigureAwait(false);

// Make Program class accessible for testing
public partial class Program { }
