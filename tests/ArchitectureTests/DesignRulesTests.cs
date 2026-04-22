using NetArchTest.Rules;
using Xunit;
using System;

namespace ArchitectureTests;

public class DesignRulesTests
{
    [Fact]
    public void Controllers_ShouldInherit_ApiControllerBase()
    {
        // Arrange
        var apiAssembly = typeof(ProjectTemplate.Api.Controllers.ApiControllerBase).Assembly;

        // Act
        var result = Types
            .InAssembly(apiAssembly)
            .That()
            .ResideInNamespace("ProjectTemplate.Api.Controllers")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveName("ApiControllerBase")
            .Should()
            .Inherit(typeof(ProjectTemplate.Api.Controllers.ApiControllerBase))
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, "Todos os controllers devem herdar de ApiControllerBase.");
    }

    [Fact]
    public void Dtos_ShouldFollow_NamingConvention()
    {
        // Arrange
        var domainAssembly = typeof(ProjectTemplate.Domain.Entities.EntityBase).Assembly;

        // Act
        var types = Types
            .InAssembly(domainAssembly)
            .That()
            .ResideInNamespace("ProjectTemplate.SharedModels")
            .And()
            .AreNotInterfaces()
            .GetTypes();

        var invalidTypes = types
            .Where(t => !t.Name.EndsWith("Dto") 
                     && !t.Name.EndsWith("Request") 
                     && !t.Name.EndsWith("Response") 
                     && t.Name != "ExceptionContext" 
                     && !t.Name.StartsWith("PagedResponse"))
            .Select(t => t.Name)
            .ToList();

        // Assert
        var failingTypes = invalidTypes.Any() ? string.Join(", ", invalidTypes) : "none";
        Assert.True(invalidTypes.Count == 0, $"Todas as classes em Domain.Dtos devem ter o sufixo 'Dto', 'Request' ou 'Response'. Falharam: {failingTypes}");
    }

    [Fact]
    public void Entities_ShouldInherit_EntityBase()
    {
        // Arrange
        var domainAssembly = typeof(ProjectTemplate.Domain.Entities.EntityBase).Assembly;

        // Act
        var types = Types
            .InAssembly(domainAssembly)
            .That()
            .ResideInNamespace("ProjectTemplate.Domain.Entities")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        var invalidTypes = types
            .Where(t => !typeof(ProjectTemplate.Domain.Entities.EntityBase).IsAssignableFrom(t)
                     && !typeof(ProjectTemplate.Domain.Entities.MongoEntityBase).IsAssignableFrom(t)
                     && t.Name != "OrderStatus"
                     && t.Name != "DomainEvent"
                     && t.Name != "UserRole"
                     && t.Name != "RefreshToken")
            .Select(t => t.Name)
            .ToList();

        // Assert
        var failingTypes = invalidTypes.Any() ? string.Join(", ", invalidTypes) : "none";
        Assert.True(invalidTypes.Count == 0, $"Todas as entidades devem herdar de EntityBase ou MongoEntityBase. Falharam: {failingTypes}");
    }
}
