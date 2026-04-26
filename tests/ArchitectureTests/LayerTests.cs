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
        var domainAssembly = typeof(ProjectTemplate.Domain.Entities.EntityBase).Assembly;

        var result = Types
            .InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, DataNamespace, ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "A camada de Domínio não deve referenciar outras camadas.");
    }

    [Fact]
    public void ApplicationLayer_ShouldOnlyDependOn_DomainAndShared()
    {
        var applicationAssembly = typeof(ProjectTemplate.Application.Services.Service<>).Assembly;

        var result = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, DataNamespace, ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            "A camada de Application deve depender apenas de Domain e Shared. " +
            "Dependências diretas em Data, Infrastructure ou Api violam a Clean Architecture.");
    }

    [Fact]
    public void DataLayer_ShouldNotHaveDependencies_OnApiOrInfrastructure()
    {
        var dataAssembly = typeof(ProjectTemplate.Data.Context.ApplicationDbContext).Assembly;

        var result = Types
            .InAssembly(dataAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApiNamespace, InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "A camada de Data não deve depender de Api ou Infrastructure.");
    }

    [Fact]
    public void InfrastructureLayer_ShouldNotHaveDependencies_OnApi()
    {
        var infrastructureAssembly = typeof(ProjectTemplate.Infrastructure.Extensions.InfrastructureExtensions).Assembly;

        var result = Types
            .InAssembly(infrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "A camada de Infrastructure não deve depender da Api.");
    }

    [Fact]
    public void SharedLayer_ShouldNotHaveDependencies_OnServerLayers()
    {
        var sharedAssembly = typeof(ProjectTemplate.Shared.Models.ExceptionContext).Assembly;

        var result = Types
            .InAssembly(sharedAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(DomainNamespace, ApplicationNamespace, InfrastructureNamespace, DataNamespace, ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            "A camada Shared não deve depender de nenhuma camada do servidor. " +
            "Shared deve ser agnóstica à arquitetura para poder ser referenciada por qualquer camada.");
    }
}
