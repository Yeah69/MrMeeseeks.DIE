using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.ScopeSpecificAttributesTestsWithImplementations;

internal interface IDependency {}

internal class DependencyContainer : IDependency {}

internal class DependencyTransientScope : IDependency {}

internal class DependencyScope : IDependency {}

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

[FilterImplementationAggregation(typeof(DependencyScope))]
[FilterImplementationAggregation(typeof(DependencyTransientScope))]
internal partial class Container : IContainer<IDependency>, IContainer<TransientScope>, IContainer<Scope>
{
    [FilterImplementationAggregation(typeof(DependencyContainer))]
    [ImplementationAggregation(typeof(DependencyTransientScope))]
    internal partial class DIE_DefaultTransientScope
    {
        
    }

    [FilterImplementationAggregation(typeof(DependencyContainer))]
    [ImplementationAggregation(typeof(DependencyScope))]
    internal partial class DIE_DefaultScope
    {
        
    }
}
public class Tests
{
    
    [Fact]
    public void Container()
    {
        using var container = new Container();
        var dependency = ((IContainer<IDependency>) container).Resolve();
        Assert.IsType<DependencyContainer>(dependency);
    }
    [Fact]
    public void TransientScope()
    {
        using var container = new Container();
        var dependency = ((IContainer<TransientScope>) container).Resolve();
        Assert.IsType<DependencyTransientScope>(dependency.Dependency);
    }
    [Fact]
    public void Scope()
    {
        using var container = new Container();
        var dependency = ((IContainer<Scope>) container).Resolve();
        Assert.IsType<DependencyScope>(dependency.Dependency);
    }
}