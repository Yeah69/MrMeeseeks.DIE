using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Decorator.Ordinal.Choice;

internal interface IInterface
{
    IInterface Decorated { get; }
}

[DecorationOrdinal(69)]
internal class DecoratorUh : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

[DecorationOrdinal(23)]
internal class DecoratorY : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

[DecorationOrdinal(3)]
internal class DecoratorE : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

internal class DecoratorA : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

[DecorationOrdinal(-1)]
internal class DecoratorH : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

internal class Dependency : IInterface
{
    public IInterface Decorated => this;
}

[CreateFunction(typeof(IInterface), "Create")]
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
        var uh = container.Create();
        var y = uh.Decorated;
        var e = y.Decorated;
        var a = e.Decorated;
        var h = a.Decorated;
        var implementation = h.Decorated;
        
        Assert.IsType<DecoratorUh>(uh);
        Assert.IsType<DecoratorY>(y);
        Assert.IsType<DecoratorE>(e);
        Assert.IsType<DecoratorA>(a);
        Assert.IsType<DecoratorH>(h);
        Assert.IsType<Dependency>(implementation);
    }
}