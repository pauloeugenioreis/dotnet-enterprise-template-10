using NetArchTest.Rules;
using Xunit;
using System;

namespace ArchitectureTests;

public class DesignRulesTests
{
    private const string InfrastructureNamespace = "ProjectTemplate.Infrastructure";

    [Fact]
    public void Controllers_ShouldInherit_ApiControllerBase()
    {
        var apiAssembly = typeof(ProjectTemplate.Api.Controllers.ApiControllerBase).Assembly;

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

        Assert.True(result.IsSuccessful, "Todos os controllers devem herdar de ApiControllerBase.");
    }

    [Fact]
    public void Controllers_ShouldNotHaveDependencies_OnInfrastructure()
    {
        var apiAssembly = typeof(ProjectTemplate.Api.Controllers.ApiControllerBase).Assembly;

        var result = Types
            .InAssembly(apiAssembly)
            .That()
            .ResideInNamespace("ProjectTemplate.Api.Controllers")
            .And()
            .AreClasses()
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Controllers não devem depender diretamente da camada de Infrastructure. " +
            "Use apenas interfaces definidas em Domain ou Application.");
    }

    [Fact]
    public void Dtos_ShouldFollow_NamingConvention()
    {
        var sharedAssembly = typeof(ProjectTemplate.Shared.Models.ExceptionContext).Assembly;

        var types = Types
            .InAssembly(sharedAssembly)
            .That()
            .ResideInNamespace("ProjectTemplate.Shared.Models")
            .And()
            .AreNotInterfaces()
            .GetTypes();

        var invalidTypes = types
            .Where(t => !t.Name.EndsWith("Dto", StringComparison.Ordinal)
                     && !t.Name.EndsWith("Request", StringComparison.Ordinal)
                     && !t.Name.EndsWith("Response", StringComparison.Ordinal)
                     && t.Name != "ExceptionContext"
                     && !t.Name.StartsWith("PagedResponse", StringComparison.Ordinal))
            .Select(t => t.Name)
            .ToList();

        var failingTypes = invalidTypes.Count > 0 ? string.Join(", ", invalidTypes) : "none";
        Assert.True(invalidTypes.Count == 0,
            $"Todas as classes em Shared.Models devem ter o sufixo 'Dto', 'Request' ou 'Response'. Falharam: {failingTypes}");
    }

    [Fact]
    public void Entities_ShouldInherit_EntityBase()
    {
        var domainAssembly = typeof(ProjectTemplate.Domain.Entities.EntityBase).Assembly;

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

        var failingTypes = invalidTypes.Count > 0 ? string.Join(", ", invalidTypes) : "none";
        Assert.True(invalidTypes.Count == 0,
            $"Todas as entidades devem herdar de EntityBase ou MongoEntityBase. Falharam: {failingTypes}");
    }

    [Fact]
    public void Services_ShouldFollowNamingConvention()
    {
        var applicationAssembly = typeof(ProjectTemplate.Application.Services.Service<>).Assembly;

        // Split on '`' to normalize generic type names (e.g. "Service`1" → "Service")
        var invalidTypes = Types
            .InAssembly(applicationAssembly)
            .That()
            .ResideInNamespace("ProjectTemplate.Application.Services")
            .And()
            .AreClasses()
            .GetTypes()
            .Where(t => !t.Name.Split('`')[0].EndsWith("Service", StringComparison.Ordinal))
            .Select(t => t.Name)
            .ToList();

        var failingTypes = invalidTypes.Count > 0 ? string.Join(", ", invalidTypes) : "none";
        Assert.True(invalidTypes.Count == 0,
            $"Todos os services da camada Application devem ter o sufixo 'Service'. Falharam: {failingTypes}");
    }

    [Fact]
    public void Services_ShouldImplement_DomainInterfaces()
    {
        var applicationAssembly = typeof(ProjectTemplate.Application.Services.Service<>).Assembly;

        var invalidTypes = Types
            .InAssembly(applicationAssembly)
            .That()
            .HaveNameEndingWith("Service", StringComparison.Ordinal)
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes()
            .Where(t => !t.GetInterfaces().Any(i => i.Namespace?.StartsWith("ProjectTemplate.Domain", StringComparison.Ordinal) == true))
            .Select(t => t.Name)
            .ToList();

        var failingTypes = invalidTypes.Count > 0 ? string.Join(", ", invalidTypes) : "none";
        Assert.True(invalidTypes.Count == 0,
            $"Todos os services devem implementar pelo menos uma interface definida no Domain. Falharam: {failingTypes}");
    }

    [Fact]
    public void Validators_ShouldFollowNamingConvention()
    {
        var domainAssembly = typeof(ProjectTemplate.Domain.Entities.EntityBase).Assembly;

        var types = Types
            .InAssembly(domainAssembly)
            .That()
            .ResideInNamespace("ProjectTemplate.Domain.Validators")
            .And()
            .AreClasses()
            .GetTypes();

        var invalidTypes = types
            .Where(t => !t.Name.EndsWith("Validator", StringComparison.Ordinal)
                     && !t.Name.EndsWith("Attribute", StringComparison.Ordinal))
            .Select(t => t.Name)
            .ToList();

        var failingTypes = invalidTypes.Count > 0 ? string.Join(", ", invalidTypes) : "none";
        Assert.True(invalidTypes.Count == 0,
            $"Todos os validators do Domain devem ter o sufixo 'Validator'. Falharam: {failingTypes}");
    }

    [Fact]
    public void Repositories_ShouldImplement_DomainInterfaces()
    {
        var dataAssembly = typeof(ProjectTemplate.Data.Context.ApplicationDbContext).Assembly;

        var invalidTypes = Types
            .InAssembly(dataAssembly)
            .That()
            .HaveNameEndingWith("Repository", StringComparison.Ordinal)
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes()
            .Where(t => !t.GetInterfaces().Any(i => i.Namespace?.StartsWith("ProjectTemplate.Domain", StringComparison.Ordinal) == true))
            .Select(t => t.Name)
            .ToList();

        var failingTypes = invalidTypes.Count > 0 ? string.Join(", ", invalidTypes) : "none";
        Assert.True(invalidTypes.Count == 0,
            $"Todos os repositories devem implementar pelo menos uma interface definida no Domain. Falharam: {failingTypes}");
    }

    [Fact]
    public void Exceptions_ShouldBeDefinedOnly_InDomain()
    {
        var infrastructureAssembly = typeof(ProjectTemplate.Infrastructure.Extensions.InfrastructureExtensions).Assembly;
        var apiAssembly = typeof(ProjectTemplate.Api.Controllers.ApiControllerBase).Assembly;

        var infrastructureExceptions = Types
            .InAssembly(infrastructureAssembly)
            .That()
            .Inherit(typeof(Exception))
            .GetTypes()
            .Select(t => t.FullName!)
            .ToList();

        var apiExceptions = Types
            .InAssembly(apiAssembly)
            .That()
            .Inherit(typeof(Exception))
            .GetTypes()
            .Select(t => t.FullName!)
            .ToList();

        Assert.True(infrastructureExceptions.Count == 0,
            $"Exceptions de negócio devem ser definidas apenas no Domain. " +
            $"Encontradas em Infrastructure: {string.Join(", ", infrastructureExceptions)}");

        Assert.True(apiExceptions.Count == 0,
            $"Exceptions de negócio devem ser definidas apenas no Domain. " +
            $"Encontradas em Api: {string.Join(", ", apiExceptions)}");
    }
}
