using ProjectTemplate.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectTemplate.Data.Context;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Interfaces;
using ProjectTemplate.Integration.Tests.Support;
using Testcontainers.PostgreSql;
using Xunit;

namespace ProjectTemplate.Integration.Tests;

/// <summary>
/// Custom WebApplicationFactory for integration tests with real PostgreSQL via Testcontainers
/// </summary>
public class WebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("ProjectTemplateTests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        // Use environment variables to ensure overrides are picked up by Program.cs
        // This is necessary because Program.cs reloads configuration from files
        // and calls .AddEnvironmentVariables() at the end.
        var connString = _dbContainer.GetConnectionString();
        System.Environment.SetEnvironmentVariable("AppSettings__Infrastructure__EventSourcing__Enabled", "true");
        System.Environment.SetEnvironmentVariable("AppSettings__Infrastructure__EventSourcing__Provider", "Custom");
        System.Environment.SetEnvironmentVariable("AppSettings__Infrastructure__EventSourcing__ConnectionString", connString);
        System.Environment.SetEnvironmentVariable("AppSettings__Infrastructure__Database__DatabaseType", "PostgreSQL");
        System.Environment.SetEnvironmentVariable("AppSettings__Infrastructure__Database__ConnectionString", connString);
        System.Environment.SetEnvironmentVariable("AppSettings__Authentication__Enabled", "false");
        System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        System.Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");

        // Trigger host creation and ensure database is created
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
        SeedTestData(db);
    }

    public override async ValueTask DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Testing to disable Swagger in Program.cs
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Authentication:Enabled"] = "false",
                ["AppSettings:Authentication:Enabled"] = "false"
            }!);
        });

        builder.ConfigureServices(services =>
        {
            // Remove ALL Swagger-related services
            var swaggerDescriptors = services
                .Where(d => d.ServiceType.Namespace != null &&
                           (d.ServiceType.Namespace.StartsWith("Swashbuckle") ||
                            d.ServiceType.Namespace.StartsWith("Microsoft.OpenApi")))
                .ToList();

            foreach (var descriptor in swaggerDescriptors)
            {
                services.Remove(descriptor);
            }

            // Replace authentication with a test handler
            var authDescriptors = services
                .Where(d => d.ServiceType.Namespace != null &&
                           (d.ServiceType.Namespace.StartsWith("Microsoft.AspNetCore.Authentication") ||
                            d.ServiceType.FullName?.Contains("Authentication") == true))
                .ToList();

            foreach (var descriptor in authDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName, _ => { });

            services.AddAuthorization();

            // Replace Marten-based event store with an in-memory implementation for tests
            services.RemoveAll(typeof(IEventStore));
            services.AddSingleton<IEventStore, InMemoryEventStore>();

            // Ensure EventSourcing settings are available
            services.RemoveAll<EventSourcingSettings>();
            services.AddSingleton(new EventSourcingSettings
            {
                Enabled = true,
                Mode = EventSourcingMode.Hybrid,
                Provider = "Custom",
                EnableAuditApi = true,
                StoreMetadata = true,
                AuditEntities = new List<string> { "Order", "Product" }
            });

            // Overwrite ApplicationDbContext to disable EnableRetryOnFailure during tests.
            // This is required because the retrying strategy does not support manual transactions (BeginTransactionAsync),
            // which are used in OrderService.cs.
            // We purge ALL services related to DbContext to ensure a truly clean re-registration.
            var toRemove = services.Where(d => 
                d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true || 
                d.ServiceType.Name.Contains("DbContext") ||
                d.ServiceType.Name.Contains("IDbContext")).ToList();
            
            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            var connectionString = _dbContainer.GetConnectionString();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(60);
                    // Retries are disabled by not calling EnableRetryOnFailure
                });
            });

            // Re-register the base DbContext mapping needed by some repositories
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            // Configure API Versioning for tests
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version"),
                    new QueryStringApiVersionReader("api-version")
                );
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
        });
    }

    public void ClearEventStore()
    {
        // For xUnit v3, we can't easily access Services before the first client is created
        // if we are in the constructor. However, we can use a lazy initialization.
        var eventStore = Services.GetService<IEventStore>() as InMemoryEventStore;
        eventStore?.Clear();
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Add seed data if needed
    }
}

