using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.ScopeSpecificAttributes.Implementation;

internal interface IDependency;

internal class DependencyContainer : IDependency;

internal class DependencyTransientScope : IDependency;

internal class DependencyScope : IDependency;

internal class TransientScope : ITransientScopeRoot
{
    public TransientScope(IDependency dependency) => Dependency = dependency;

    public IDependency Dependency { get; }
}

internal class Scope : IScopeRoot
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
public class Tests
{
    [Fact]
    public void Container()
    {
        using var container = Implementation.Container.DIE_CreateContainer();
        var dependency = container.Create0();
        Assert.IsType<DependencyContainer>(dependency);
    }
    [Fact]
    public void TransientScope()
    {
        using var container = Implementation.Container.DIE_CreateContainer();
        var dependency = container.Create1();
        Assert.IsType<DependencyTransientScope>(dependency.Dependency);
    }
    [Fact]
    public void Scope()
    {
        using var container = Implementation.Container.DIE_CreateContainer();
        var dependency = container.Create2();
        Assert.IsType<DependencyScope>(dependency.Dependency);
    }
}