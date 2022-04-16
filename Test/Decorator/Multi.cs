using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Decorator.Multi;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal class Dependency : IInterface
{
    public IInterface Decorated => this;
}

internal class DecoratorA : IInterface, IDecorator<IInterface>
{
    public DecoratorA(IInterface decorated) =>
        Decorated = decorated;

    public IInterface Decorated { get; }
}

internal class DecoratorB : IInterface, IDecorator<IInterface>
{
    public DecoratorB(IInterface decorated) =>
        Decorated = decorated;

    public IInterface Decorated { get; }
}

[CreateFunction(typeof(IInterface), "Create")]
[DecoratorSequenceChoice(typeof(IInterface), typeof(DecoratorA), typeof(DecoratorB))]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var decorated = container.Create();
        var decoratedB = decorated;
        var decoratedA = decorated.Decorated;
        var decoratedBasis = decoratedA.Decorated;
        Assert.NotEqual(decoratedBasis, decoratedA);
        Assert.NotEqual(decoratedBasis, decoratedB);
        Assert.NotEqual(decoratedA, decoratedB);
        Assert.IsType<Dependency>(decoratedBasis);
        Assert.IsType<DecoratorA>(decoratedA);
        Assert.IsType<DecoratorB>(decoratedB);
    }
}