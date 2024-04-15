using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.ScopeSpecificAttributes.Implementation;

internal interface IDependency;

internal sealed class DependencyContainer : IDependency;

internal sealed class DependencyTransientScope : IDependency;

internal sealed class DependencyScope : IDependency;

internal sealed class TransientScope : ITransientScopeRoot
{
    public TransientScope(IDependency dependency) => Dependency = dependency;

    public IDependency Dependency { get; }
}

internal sealed class Scope : IScopeRoot
{
    public Scope(IDependency dependency) => Dependency = dependency;

    public IDependency Dependency { get; }
}

[CreateFunction(typeof(IDependency), "Create0")]
[CreateFunction(typeof(TransientScope), "Create1")]
[CreateFunction(typeof(Scope), "Create2")]
[FilterImplementationAggregation(typeof(DependencyScope))]
[FilterImplementationAggregation(typeof(DependencyTransientScope))]
internal sealed partial class Container
{
    
    
    [FilterImplementationAggregation(typeof(DependencyContainer))]
    [ImplementationAggregation(typeof(DependencyTransientScope))]
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultTransientScope;

    [FilterImplementationAggregation(typeof(DependencyContainer))]
    [ImplementationAggregation(typeof(DependencyScope))]
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultScope;
}
public sealed class Tests
{
    [Fact]
    public async Task Container()
    {
        await using var container = Implementation.Container.DIE_CreateContainer();
        var dependency = container.Create0();
        Assert.IsType<DependencyContainer>(dependency);
    }
    [Fact]
    public async Task TransientScope()
    {
        await using var container = Implementation.Container.DIE_CreateContainer();
        var dependency = container.Create1();
        Assert.IsType<DependencyTransientScope>(dependency.Dependency);
    }
    [Fact]
    public async Task Scope()
    {
        await using var container = Implementation.Container.DIE_CreateContainer();
        var dependency = container.Create2();
        Assert.IsType<DependencyScope>(dependency.Dependency);
    }
}