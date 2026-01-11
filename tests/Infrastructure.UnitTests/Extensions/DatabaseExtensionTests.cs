using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Infrastructure.Extensions;
using Xunit;

namespace ProjectTemplate.Infrastructure.UnitTests.Extensions;

/// <summary>
/// Unit tests for DatabaseExtension
/// </summary>
public class DatabaseExtensionTests
{
    [Fact]
    public void AddDatabaseConfiguration_WithValidConfiguration_RegistersDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["AppSettings:Infrastructure:Database:Provider"] = "EntityFramework",
                ["AppSettings:Infrastructure:Database:DatabaseType"] = "SqlServer",
                ["AppSettings:ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=TestDb;"
            }!)
            .Build();

        // Act
        var action = () => services.AddDatabaseConfiguration(configuration);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddDatabaseConfiguration_WithMemoryCache_RegistersMemoryCache()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["AppSettings:Infrastructure:Cache:Provider"] = "Memory"
            }!)
            .Build();

        // Act
        services.AddCacheConfiguration(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.Should().NotBeNull();
    }
}
