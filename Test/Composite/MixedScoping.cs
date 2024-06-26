using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Composite.MixedScoping;

internal interface IInterface
{
    IReadOnlyList<IInterface> Composites { get; }
}

internal sealed class BasisA : IInterface, IContainerInstance
{
    public IReadOnlyList<IInterface> Composites => new List<IInterface> { this };
}

internal sealed class BasisB : IInterface
{
    public IReadOnlyList<IInterface> Composites => new List<IInterface> { this };
}

internal sealed class Composite : IInterface, IComposite<IInterface>
{
    public Composite(IReadOnlyList<IInterface> composites) => 
        Composites = composites;

    public IReadOnlyList<IInterface> Composites { get; }
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
        var nextComposite = container.CreateDep();
        Assert.NotEqual(composite, nextComposite);
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
        var nextComposites = container.CreateCollection();
        Assert.True(composites[0].Equals(nextComposites[0]) && !composites[1].Equals(nextComposites[1])
            || composites[1].Equals(nextComposites[1]) && !composites[0].Equals(nextComposites[0]));
    }
}