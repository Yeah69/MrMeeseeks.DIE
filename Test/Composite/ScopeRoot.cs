using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Composite.ScopeRoot;

internal interface IInterface
{
    IReadOnlyList<IInterface> Composites { get; }
    IDependency Dependency { get; }
}

internal interface IDependency;

internal sealed class Dependency : IDependency, IScopeInstance;

internal sealed class BasisA : IInterface, IScopeRoot
{
    public BasisA(IDependency dependency) => Dependency = dependency;

    public IReadOnlyList<IInterface> Composites => new List<IInterface> { this };
    public IDependency Dependency { get; }
}

internal sealed class BasisB : IInterface
{
    public BasisB(IDependency dependency) => Dependency = dependency;

    public IReadOnlyList<IInterface> Composites => new List<IInterface> { this };
    public IDependency Dependency { get; }
}

internal sealed class Composite : IInterface, IComposite<IInterface>
{
    public Composite(IReadOnlyList<IInterface> composites, IDependency dependency)
    {
        Composites = composites;
        Dependency = dependency;
    }

    public IReadOnlyList<IInterface> Composites { get; }
    public IDependency Dependency { get; }
}

[CreateFunction(typeof(IInterface), "CreateDep")]
[CreateFunction(typeof(IReadOnlyList<IInterface>), "CreateCollection")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var composite = container.CreateDep();
        foreach (var compositeComposite in composite.Composites)
        {
            Assert.NotEqual(composite, compositeComposite);
            var type = compositeComposite.GetType();
            Assert.True(type == typeof(BasisA) || type == typeof(BasisB));
        }
        Assert.Equal(2, composite.Composites.Count);
        Assert.NotEqual(composite.Composites[0].Dependency, composite.Composites[1].Dependency);
    }
    
    [Fact]
    public async Task TestList()
    {
        await using var container = Container.DIE_CreateContainer();
        var composites = container.CreateCollection();
        foreach (var compositeComposite in composites)
        {
            var type = compositeComposite.GetType();
            Assert.True(type == typeof(BasisA) || type == typeof(BasisB));
        }
        Assert.Equal(2, composites.Count);
        Assert.NotEqual(composites[0].Dependency, composites[1].Dependency);
    }
}