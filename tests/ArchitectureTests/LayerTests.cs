using NetArchTest.Rules;
using Xunit;

namespace ArchitectureTests;

public class LayerTests
{
    private const string DomainNamespace = "ProjectTemplate.Domain";
    private const string ApplicationNamespace = "ProjectTemplate.Application";
    private const string InfrastructureNamespace = "ProjectTemplate.Infrastructure";
    private const string DataNamespace = "ProjectTemplate.Data";
    private const string ApiNamespace = "ProjectTemplate.Api";

    [Fact]
    public void DomainLayer_ShouldNotHaveDependencies_OnOtherLayers()
    {
        // Arrange
        var domainAssembly = typeof(ProjectTemplate.Domain.Entities.EntityBase).Assembly;

        // Act
        var result = Types
            .InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, DataNamespace, ApiNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, "A camada de Domínio não deve referenciar outras camadas.");
    }

    [Fact]
    public void ApplicationLayer_ShouldNotHaveDependencies_OnInfrastructureOrData()
    {
        // Arrange
        var applicationAssembly = typeof(ProjectTemplate.Application.Services.Service<>).Assembly;

        // Act
        var result = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, DataNamespace, ApiNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, "A camada de Aplicação não deve depender diretamente de Infrastructure ou Data.");
    }

    [Fact]
    public void DataLayer_ShouldNotHaveDependencies_OnApi()
    {
        // Arrange
        var dataAssembly = typeof(ProjectTemplate.Data.Context.ApplicationDbContext).Assembly;

        // Act
        var result = Types
            .InAssembly(dataAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApiNamespace, InfrastructureNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, "A camada de Data não deve depender de Api ou Infrastructure.");
    }

    [Fact]
    public void InfrastructureLayer_ShouldNotHaveDependencies_OnApi()
    {
        // Arrange
        var infrastructureAssembly = typeof(ProjectTemplate.Infrastructure.Extensions.InfrastructureExtensions).Assembly;

        // Act
        var result = Types
            .InAssembly(infrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, "A camada de Infrastructure não deve depender da Api.");
    }
}
