using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
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
    public async ValueTask Container()
    {
        await using var container = new Container();
        var dependencies = container.Create0();
        Assert.Equal(3, dependencies.Count);
        Assert.Contains(dependencies, d => d.GetType() == typeof(DependencyContainer));
        Assert.Contains(dependencies, d => d.GetType() == typeof(DependencyTransientScope));
        Assert.Contains(dependencies, d => d.GetType() == typeof(DependencyScope));
    }
    [Fact]
    public async ValueTask TransientScope()
    {
        await using var container = new Container();
        var dependencies = container.Create1();
        Assert.Equal(1, dependencies.Dependencies.Count);
        Assert.Contains(dependencies.Dependencies, d => d.GetType() == typeof(DependencyTransientScope));
    }
    [Fact]
    public async ValueTask Scope()
    {
        await using var container = new Container();
        var dependencies = container.Create2();
        Assert.Equal(1, dependencies.Dependencies.Count);
        Assert.Contains(dependencies.Dependencies, d => d.GetType() == typeof(DependencyScope));
    }
}