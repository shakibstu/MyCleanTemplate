using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace MyCleanTemplate.ArchitectureTest;

public class LayeringTests
{
    private const string DomainAssembly = "MyCleanTemplate.Domain";
    private const string ApplicationAssembly = "MyCleanTemplate.Application";
    private const string ApplicationContractAssembly = "MyCleanTemplate.Application.Contract";
    private const string InfrastructureAssembly = "MyCleanTemplate.Infrastructure";
    private const string ApiAssembly = "MyCleanTemplate.API";

    [Fact]
    public void Domain_should_not_depend_on_Application_Infrastructure_or_Api()
    {
        var result = Types.InAssembly(typeof(Domain.Common.BaseEntity).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationAssembly, ApplicationContractAssembly, InfrastructureAssembly, ApiAssembly)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(BuildFailure(result));
    }

    [Fact]
    public void ApplicationContract_should_not_depend_on_Infrastructure_or_Api()
    {
        var result = Types.InAssembly(typeof(Application.Contract.Common.Paging.PageRequest).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureAssembly, ApiAssembly)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(BuildFailure(result));
    }

    [Fact]
    public void Application_should_not_depend_on_Infrastructure_or_Api()
    {
        var result = Types.InAssembly(typeof(Application.ApplicationServiceRegistration).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureAssembly, ApiAssembly)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(BuildFailure(result));
    }

    [Fact]
    public void Infrastructure_should_not_depend_on_Api()
    {
        var result = Types.InAssembly(typeof(Infrastructure.InfrastructureServiceRegistration).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiAssembly)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(BuildFailure(result));
    }

    [Fact]
    public void Controllers_should_live_in_the_Api_assembly()
    {
        var result = Types.InAssemblies(
            [
                typeof(Application.ApplicationServiceRegistration).Assembly,
                typeof(Infrastructure.InfrastructureServiceRegistration).Assembly,
                typeof(Domain.Common.BaseEntity).Assembly
            ])
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .ResideInNamespace(ApiAssembly)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(BuildFailure(result));
    }

    private static string BuildFailure(NetArchTest.Rules.TestResult result) =>
        result.FailingTypeNames is null
            ? "Architecture rule violated."
            : "Architecture rule violated by:\n  " + string.Join("\n  ", result.FailingTypeNames);
}
