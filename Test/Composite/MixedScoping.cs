using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Composite.MixedScoping;

internal interface IInterface
{
    IReadOnlyList<IInterface> Composites { get; }
}

internal class BasisA : IInterface, IContainerInstance
{
    public IReadOnlyList<IInterface> Composites => new List<IInterface> { this };
}

internal class BasisB : IInterface
{
    public IReadOnlyList<IInterface> Composites => new List<IInterface> { this };
}

internal class Composite : IInterface, IComposite<IInterface>
{
    public Composite(IReadOnlyList<IInterface> composites) => 
        Composites = composites;

    public IReadOnlyList<IInterface> Composites { get; }
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
        var nextComposite = container.CreateDep();
        Assert.NotEqual(composite, nextComposite);
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
        var nextComposites = container.CreateCollection();
        Assert.True(composites[0].Equals(nextComposites[0]) && !composites[1].Equals(nextComposites[1])
            || composites[1].Equals(nextComposites[1]) && !composites[0].Equals(nextComposites[0]));
    }
}