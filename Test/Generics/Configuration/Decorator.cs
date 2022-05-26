using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.Decorator;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal class Implementation : IInterface
{
    public IInterface Decorated => this;
}

internal class DecoratorA<T0> : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated { get; }

    internal DecoratorA(
        IInterface decorated) =>
        Decorated = decorated;
}

internal class DecoratorB<T0> : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated { get; }

    internal DecoratorB(
        IInterface decorated) =>
        Decorated = decorated;
}

[GenericParameterChoice(typeof(DecoratorA<>), "T0", typeof(int))]
[GenericParameterChoice(typeof(DecoratorB<>), "T0", typeof(string))]
[DecoratorSequenceChoice(typeof(IInterface), typeof(DecoratorA<>), typeof(DecoratorB<>))]
[CreateFunction(typeof(IInterface), "Create")]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var decorator = container.Create();
        Assert.IsType<DecoratorB<string>>(decorator);
        Assert.IsType<DecoratorA<int>>(decorator.Decorated);
        Assert.IsType<Implementation>(decorator.Decorated.Decorated);
    }
}