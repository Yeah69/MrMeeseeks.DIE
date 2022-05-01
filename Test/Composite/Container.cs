using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Composite.Container;

internal interface IInterface
{
    IReadOnlyList<IInterface> Composites { get; }
}

internal class BasisA : IInterface, IContainerInstance
{
    public IReadOnlyList<IInterface> Composites => new List<IInterface> { this };
}

internal class BasisB : IInterface, IContainerInstance
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
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var composite = container.CreateDep();
        foreach (var compositeComposite in composite.Composites)
        {
            Assert.NotEqual(composite, compositeComposite);
            var type = compositeComposite.GetType();
            Assert.True(type == typeof(BasisA) || type == typeof(BasisB));
        }
        var nextComposite = container.CreateDep();
        Assert.Equal(composite, nextComposite);
    }
    
    [Fact]
    public void TestList()
    {
        var container = new Container();
        var composites = container.CreateCollection();
        foreach (var compositeComposite in composites)
        {
            var type = compositeComposite.GetType();
            Assert.True(type == typeof(BasisA) || type == typeof(BasisB));
        }
        Assert.Equal(2, composites.Count);
        var nextComposites = container.CreateCollection();
        Assert.Equal(composites[0], nextComposites[0]);
        Assert.Equal(composites[1], nextComposites[1]);
    }
}