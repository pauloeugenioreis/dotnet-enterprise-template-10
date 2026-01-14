using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using ProjectTemplate.Data.Context;
using Asp.Versioning;

namespace ProjectTemplate.Integration.Tests;

/// <summary>
/// Custom WebApplicationFactory for integration tests with in-memory database
/// </summary>
public class WebApplicationFactoryFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Testing to disable Swagger in Program.cs
        builder.UseEnvironment("Testing");
        
        // Add test configuration with valid JWT secret
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Authentication:Enabled"] = "false"
            }!);
        });
        
        builder.ConfigureServices(services =>
        {
            // Remove ALL Swagger-related services to avoid OpenAPI version conflicts
            var swaggerDescriptors = services
                .Where(d => d.ServiceType.Namespace != null && 
                           (d.ServiceType.Namespace.StartsWith("Swashbuckle") ||
                            d.ServiceType.Namespace.StartsWith("Microsoft.OpenApi")))
                .ToList();
            
            foreach (var descriptor in swaggerDescriptors)
            {
                services.Remove(descriptor);
            }
            
            // Remove ALL Authentication services to avoid JWT configuration issues
            var authDescriptors = services
                .Where(d => d.ServiceType.Namespace != null && 
                           (d.ServiceType.Namespace.StartsWith("Microsoft.AspNetCore.Authentication") ||
                            d.ServiceType.FullName?.Contains("Authentication") == true))
                .ToList();
            
            foreach (var descriptor in authDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove the existing DbContext registration
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));
            services.RemoveAll(typeof(DbContext));

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            // Register DbContext as ApplicationDbContext for Repository<T> that expects DbContext
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

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();

            // Seed test data if needed
            SeedTestData(db);
        });
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Add seed data for tests
        // Example:
        // context.Products.Add(new Product { Name = "Test Product", Price = 10.00m });
        // context.SaveChanges();
    }
}
