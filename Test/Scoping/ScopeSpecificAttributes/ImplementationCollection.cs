using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.ScopeSpecificAttributes.ImplementationCollection;

internal interface IDependency;

internal sealed class DependencyContainer : IDependency;

internal sealed class DependencyTransientScope : IDependency;

internal sealed class DependencyScope : IDependency;

internal sealed class TransientScope : ITransientScopeRoot
{
    public TransientScope(IReadOnlyList<IDependency> dependencies) => Dependencies = dependencies;

    public IReadOnlyList<IDependency> Dependencies { get; }
}

internal sealed class Scope : IScopeRoot
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
    private sealed partial class DIE_DefaultTransientScope;

    [FilterImplementationAggregation(typeof(DependencyContainer))]
    [FilterImplementationAggregation(typeof(DependencyTransientScope))]
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultScope;
}
public sealed class Tests
{
    
    [Fact]
    public async Task Container()
    {
        await using var container = ImplementationCollection.Container.DIE_CreateContainer();
        var dependencies = container.Create0();
        Assert.Equal(3, dependencies.Count);
        Assert.Contains(dependencies, d => d is DependencyContainer);
        Assert.Contains(dependencies, d => d is DependencyTransientScope);
        Assert.Contains(dependencies, d => d is DependencyScope);
    }
    [Fact]
    public async Task TransientScope()
    {
        await using var container = ImplementationCollection.Container.DIE_CreateContainer();
        var dependencies = container.Create1();
        Assert.Single(dependencies.Dependencies);
        Assert.Contains(dependencies.Dependencies, d => d is DependencyTransientScope);
    }
    [Fact]
    public async Task Scope()
    {
        await using var container = ImplementationCollection.Container.DIE_CreateContainer();
        var dependencies = container.Create2();
        Assert.Single(dependencies.Dependencies);
        Assert.Contains(dependencies.Dependencies, d => d is DependencyScope);
    }
}