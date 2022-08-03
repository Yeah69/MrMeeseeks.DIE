using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.Decorator.ContainerInstanceMultipeImplementations;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal class DependencyA : IInterface, IContainerInstance
{
    public IInterface Decorated => this;
}

internal class DependencyB : IInterface, IContainerInstance
{
    public IInterface Decorated => this;
}

internal class Decorator : IInterface, IDecorator<IInterface>
{
    public Decorator(IInterface decoratedContainerInstance) => 
        Decorated = decoratedContainerInstance;

    public IInterface Decorated { get; }
}

[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
internal sealed partial class Container
{
    
}