using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.InterfaceGenericDecorator;

internal interface IInterface<T0>
{
    IInterface<T0> Decorated { get; }
}

internal class Implementation<T0> : IInterface<T0>
{
    public IInterface<T0> Decorated => this;
}

internal class DecoratorA<T0, T1> : IInterface<T1>, IDecorator<IInterface<T1>>
{
    public IInterface<T1> Decorated { get; }

    internal DecoratorA(
        IInterface<T1> decorated) =>
        Decorated = decorated;
}

internal class DecoratorB<T0, T1> : IInterface<T1>, IDecorator<IInterface<T1>>
{
    public IInterface<T1> Decorated { get; }

    internal DecoratorB(
        IInterface<T1> decorated) =>
        Decorated = decorated;
}

[GenericParameterChoice(typeof(DecoratorA<,>), "T0", typeof(int))]
[GenericParameterChoice(typeof(DecoratorB<,>), "T0", typeof(string))]
[DecoratorSequenceChoice(typeof(IInterface<>), typeof(DecoratorA<,>), typeof(DecoratorB<,>))]
[CreateFunction(typeof(IInterface<byte>), "Create")]
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
        Assert.IsType<DecoratorB<string, byte>>(decorator);
        Assert.IsType<DecoratorA<int, byte>>(decorator.Decorated);
        Assert.IsType<Implementation<byte>>(decorator.Decorated.Decorated);
    }
}