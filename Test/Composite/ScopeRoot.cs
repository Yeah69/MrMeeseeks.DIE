using System.Collections.Generic;
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

internal interface IDependency { }

internal class Dependency : IDependency, IScopeInstance { }

internal class BasisA(IDependency dependency) : IInterface, IScopeRoot
{
    public IReadOnlyList<IInterface> Composites => new List<IInterface> { this };
    public IDependency Dependency { get; } = dependency;
}

internal class BasisB(IDependency dependency) : IInterface
{
    public IReadOnlyList<IInterface> Composites => new List<IInterface> { this };
    public IDependency Dependency { get; } = dependency;
}

internal class Composite(IReadOnlyList<IInterface> composites, IDependency dependency)
    : IInterface, IComposite<IInterface>
{
    public IReadOnlyList<IInterface> Composites { get; } = composites;
    public IDependency Dependency { get; } = dependency;
}

[CreateFunction(typeof(IInterface), "CreateDep")]
[CreateFunction(typeof(IReadOnlyList<IInterface>), "CreateCollection")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
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
    public void TestList()
    {
        using var container = Container.DIE_CreateContainer();
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