using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Scoping.ScopeSpecificAttributes.ImplementationCollection;

internal interface IDependency {}

internal class DependencyContainer : IDependency {}

internal class DependencyTransientScope : IDependency {}

internal class DependencyScope : IDependency {}

internal class TransientScope : ITransientScopeRoot
{
    public TransientScope(IReadOnlyList<IDependency> dependencies) => Dependencies = dependencies;

    public IReadOnlyList<IDependency> Dependencies { get; }
}

internal class Scope : IScopeRoot
{
    public Scope(IReadOnlyList<IDependency> dependencies) => Dependencies = dependencies;

    public IReadOnlyList<IDependency> Dependencies { get; }
}

[CreateFunction(typeof(IReadOnlyList<IDependency>), "Create0")]
[CreateFunction(typeof(TransientScope), "Create1")]
[CreateFunction(typeof(Scope), "Create2")]
internal partial class Container
{
    [FilterImplementationAggregation(typeof(DependencyContainer))]
    [FilterImplementationAggregation(typeof(DependencyScope))]
    partial class DIE_DefaultTransientScope
    {
        
    }

    [FilterImplementationAggregation(typeof(DependencyContainer))]
    [FilterImplementationAggregation(typeof(DependencyTransientScope))]
    partial class DIE_DefaultScope
    {
        
    }
}
public class Tests
{
    
    [Fact]
    public void Container()
    {
        var container = new Container();
        var dependencies = container.Create0();
        Assert.Equal(3, dependencies.Count);
        Assert.Contains(dependencies, d => d.GetType() == typeof(DependencyContainer));
        Assert.Contains(dependencies, d => d.GetType() == typeof(DependencyTransientScope));
        Assert.Contains(dependencies, d => d.GetType() == typeof(DependencyScope));
    }
    [Fact]
    public void TransientScope()
    {
        var container = new Container();
        var dependencies = container.Create1();
        Assert.Equal(1, dependencies.Dependencies.Count);
        Assert.Contains(dependencies.Dependencies, d => d.GetType() == typeof(DependencyTransientScope));
    }
    [Fact]
    public void Scope()
    {
        var container = new Container();
        var dependencies = container.Create2();
        Assert.Equal(1, dependencies.Dependencies.Count);
        Assert.Contains(dependencies.Dependencies, d => d.GetType() == typeof(DependencyScope));
    }
}