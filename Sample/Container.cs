using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal class DecoratorH : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

internal class DecoratorE : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

internal class DecoratorUh : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

internal class DecoratorY : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

[DecorationOrdinal(2)]
internal class DecoratorA : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

internal class Dependency : IInterface
{
    public IInterface Decorated => this;
}

[DecorationOrdinalChoice(typeof(DecoratorH), -1)]
[DecorationOrdinalChoice(typeof(DecoratorE), 3)]
[DecorationOrdinalChoice(typeof(DecoratorY), 23)]
[DecorationOrdinalChoice(typeof(DecoratorUh), 69)]
[CreateFunction(typeof(IInterface), "Create")]
internal abstract class ContainerBase
{
}

internal sealed partial class Container : ContainerBase
{
    private Container() {}
}