using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Decorator.MultiOnConcrete;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal class Dependency : IInterface
{
    public IInterface Decorated => this;
}

internal class DecoratorA(IInterface decorated) : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated { get; } = decorated;
}

internal class DecoratorB(IInterface decorated) : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated { get; } = decorated;
}

[CreateFunction(typeof(IInterface), "Create")]
[DecoratorSequenceChoice(typeof(IInterface), typeof(Dependency), typeof(DecoratorA), typeof(DecoratorB))]
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