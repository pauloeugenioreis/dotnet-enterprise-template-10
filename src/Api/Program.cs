using ProjectTemplate.Infrastructure.Extensions;
using ProjectTemplate.Data.Context;
using ProjectTemplate.Data.Seeders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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

app.Run();

// Make Program class accessible for testing
public partial class Program { }
