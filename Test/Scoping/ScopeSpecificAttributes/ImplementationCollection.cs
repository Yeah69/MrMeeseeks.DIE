using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
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
internal sealed partial class Container
{
    
    
    [FilterImplementationAggregation(typeof(DependencyContainer))]
    [FilterImplementationAggregation(typeof(DependencyScope))]
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultTransientScope
    {
        
    }

    [FilterImplementationAggregation(typeof(DependencyContainer))]
    [FilterImplementationAggregation(typeof(DependencyTransientScope))]
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultScope
    {
        
    }
}
public class Tests
{
    
    [Fact]
    public void Container()
    {
        using var container = ImplementationCollection.Container.DIE_CreateContainer();
        var dependencies = container.Create0();
        Assert.Equal(3, dependencies.Count);
        Assert.Contains(dependencies, d => d.GetType() == typeof(DependencyContainer));
        Assert.Contains(dependencies, d => d.GetType() == typeof(DependencyTransientScope));
        Assert.Contains(dependencies, d => d.GetType() == typeof(DependencyScope));
    }
    [Fact]
    public void TransientScope()
    {
        using var container = ImplementationCollection.Container.DIE_CreateContainer();
        var dependencies = container.Create1();
        Assert.Single(dependencies.Dependencies);
        Assert.Contains(dependencies.Dependencies, d => d.GetType() == typeof(DependencyTransientScope));
    }
    [Fact]
    public void Scope()
    {
        using var container = ImplementationCollection.Container.DIE_CreateContainer();
        var dependencies = container.Create2();
        Assert.Single(dependencies.Dependencies);
        Assert.Contains(dependencies.Dependencies, d => d.GetType() == typeof(DependencyScope));
    }
}