using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;

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
[DecoratorSequenceChoice(typeof(IInterface), typeof(IInterface), typeof(DecoratorA), typeof(DecoratorB))]
internal sealed partial class Container
{
    
}