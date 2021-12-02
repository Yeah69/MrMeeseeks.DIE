using System.Collections.Generic;

namespace MrMeeseeks.DIE.Sample;

internal interface IDecoratedMulti
{
    IDecoratedMulti Decorated { get; }
}

internal class DecoratorMultiBasisA : IDecoratedMulti
{
    public IDecoratedMulti Decorated => this;
}

internal class DecoratorMultiBasisB : IDecoratedMulti
{
    public IDecoratedMulti Decorated => this;
}

internal class DecoratorMultiA : IDecoratedMulti, IDecorator<IDecoratedMulti>
{
    public DecoratorMultiA(IDecoratedMulti decorated) =>
        Decorated = decorated;

    public IDecoratedMulti Decorated { get; }
}

internal class DecoratorMultiB : IDecoratedMulti, IDecorator<IDecoratedMulti>
{
    public DecoratorMultiB(IDecoratedMulti decorated) =>
        Decorated = decorated;

    public IDecoratedMulti Decorated { get; }
}

internal partial class DecoratorMultiContainer : IContainer<IReadOnlyList<IDecoratedMulti>>
{
    
}