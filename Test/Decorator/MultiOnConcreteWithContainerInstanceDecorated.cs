using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Decorator.MultiOnConcreteWithContainerInstanceDecorated;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal sealed class Dependency : IInterface, IContainerInstance
{
    public IInterface Decorated => this;
}

internal sealed class DecoratorA : IInterface, IDecorator<IInterface>
{
    public DecoratorA(IInterface decorated) =>
        Decorated = decorated;

    public IInterface Decorated { get; }
}

internal sealed class DecoratorB : IInterface, IDecorator<IInterface>
{
    public DecoratorB(IInterface decorated) =>
        Decorated = decorated;

    public IInterface Decorated { get; }
}

[CreateFunction(typeof(IInterface), "Create")]
[DecoratorSequenceChoice(typeof(IInterface), typeof(Dependency), typeof(DecoratorA), typeof(DecoratorB))]
internal sealed partial class Container;

public sealed class Tests
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