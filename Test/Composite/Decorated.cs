using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Composite.Decorated;

internal interface IInterface
{
    IReadOnlyList<IInterface> Composites { get; }
    IInterface Decorated { get; }
}

internal sealed class DecoratorA : IInterface, IDecorator<IInterface>
{
    public IReadOnlyList<IInterface> Composites => Decorated.Composites;
    public IInterface Decorated { get; }

    public DecoratorA(
        IInterface decorated) =>
        Decorated = decorated;
}

internal sealed class DecoratorB : IInterface, IDecorator<IInterface>
{
    public IReadOnlyList<IInterface> Composites => Decorated.Composites;
    public IInterface Decorated { get; }

    public DecoratorB(
        IInterface decorated) =>
        Decorated = decorated;
}

internal sealed class BasisA : IInterface
{
    public IReadOnlyList<IInterface> Composites => new List<IInterface> { this };
    public IInterface Decorated => this;
}

internal sealed class BasisB : IInterface
{
    public IReadOnlyList<IInterface> Composites => new List<IInterface> { this };
    public IInterface Decorated => this;
}

internal sealed class Composite : IInterface, IComposite<IInterface>
{
    public Composite(IReadOnlyList<IInterface> composites) => 
        Composites = composites;

    public IReadOnlyList<IInterface> Composites { get; }
    public IInterface Decorated => this;
}

[DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface), typeof(DecoratorA), typeof(DecoratorB))]
[DecoratorSequenceChoice(typeof(IInterface), typeof(Composite), typeof(DecoratorB))]
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
        Assert.IsType<DecoratorB>(composite);
        Assert.IsType<Composite>(composite.Decorated);
        foreach (var compositeComposite in composite.Composites)
        {
            Assert.NotEqual(composite, compositeComposite);
            Assert.IsType<DecoratorB>(compositeComposite);
            Assert.IsType<DecoratorA>(compositeComposite.Decorated);
            var baseImpl = compositeComposite.Decorated.Decorated;
            Assert.True(baseImpl.GetType() == typeof(BasisA) || baseImpl.GetType() == typeof(BasisB));
        }
    }
    
    [Fact]
    public async Task TestList()
    {
        await using var container = Container.DIE_CreateContainer();
        var composites = container.CreateCollection();
        foreach (var compositeComposite in composites)
        {
            Assert.IsType<DecoratorB>(compositeComposite);
            Assert.IsType<DecoratorA>(compositeComposite.Decorated);
            var baseImpl = compositeComposite.Decorated.Decorated;
            Assert.True(baseImpl.GetType() == typeof(BasisA) || baseImpl.GetType() == typeof(BasisB));
        }
        Assert.Equal(2, composites.Count);
    }
}