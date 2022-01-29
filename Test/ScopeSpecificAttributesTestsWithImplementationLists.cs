using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.ScopeSpecificAttributesTestsWithImplementationLists;

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

internal partial class Container : IContainer<IReadOnlyList<IDependency>>, IContainer<TransientScope>, IContainer<Scope>
{
    [FilterImplementationAggregation(typeof(DependencyContainer))]
    [FilterImplementationAggregation(typeof(DependencyScope))]
    internal partial class DIE_DefaultTransientScope
    {
        
    }

    [FilterImplementationAggregation(typeof(DependencyContainer))]
    [FilterImplementationAggregation(typeof(DependencyTransientScope))]
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
        var dependencies = ((IContainer<IReadOnlyList<IDependency>>) container).Resolve();
        Assert.Equal(3, dependencies.Count);
        Assert.Contains(dependencies, d => d.GetType() == typeof(DependencyContainer));
        Assert.Contains(dependencies, d => d.GetType() == typeof(DependencyTransientScope));
        Assert.Contains(dependencies, d => d.GetType() == typeof(DependencyScope));
    }
    [Fact]
    public void TransientScope()
    {
        using var container = new Container();
        var dependencies = ((IContainer<TransientScope>) container).Resolve();
        Assert.Equal(1, dependencies.Dependencies.Count);
        Assert.Contains(dependencies.Dependencies, d => d.GetType() == typeof(DependencyTransientScope));
    }
    [Fact]
    public void Scope()
    {
        using var container = new Container();
        var dependencies = ((IContainer<Scope>) container).Resolve();
        Assert.Equal(1, dependencies.Dependencies.Count);
        Assert.Contains(dependencies.Dependencies, d => d.GetType() == typeof(DependencyScope));
    }
}