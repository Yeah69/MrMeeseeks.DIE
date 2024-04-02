using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Configuration.Decorator;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal sealed class Implementation : IInterface
{
    public IInterface Decorated => this;
}

// ReSharper disable once UnusedTypeParameter
internal sealed class DecoratorA<T0> : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated { get; }

    internal DecoratorA(
        IInterface decorated) =>
        Decorated = decorated;
}

// ReSharper disable once UnusedTypeParameter
internal sealed class DecoratorB<T0> : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated { get; }

    internal DecoratorB(
        IInterface decorated) =>
        Decorated = decorated;
}

[GenericParameterChoice(typeof(DecoratorA<>), "T0", typeof(int))]
[GenericParameterChoice(typeof(DecoratorB<>), "T0", typeof(string))]
[DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface), typeof(DecoratorA<>), typeof(DecoratorB<>))]
[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var decorator = container.Create();
        Assert.IsType<DecoratorB<string>>(decorator);
        Assert.IsType<DecoratorA<int>>(decorator.Decorated);
        Assert.IsType<Implementation>(decorator.Decorated.Decorated);
    }
}