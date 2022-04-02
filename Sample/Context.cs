using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.Decorator.SequenceEdgeCase;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal class Dependency : IInterface, IContainerInstance
{
    public IInterface Decorated => this;
}

internal class DecoratorA : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated { get; }
    
    internal DecoratorA(IInterface decorated) => Decorated = decorated;
}

internal class DecoratorB : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated { get; }
    
    internal DecoratorB(IInterface decorated) => Decorated = decorated;
}

internal class ScopeRoot0 : IScopeRoot
{
    public IInterface Decorated { get; }

    internal ScopeRoot0(IInterface decorated) => Decorated = decorated;
}

internal class ScopeRoot1 : IScopeRoot
{
    public IInterface Decorated { get; }

    internal ScopeRoot1(IInterface decorated) => Decorated = decorated;
}

[CreateFunction(typeof(ScopeRoot0), "Create0")]
[CreateFunction(typeof(ScopeRoot1), "Create1")]
[DecoratorSequenceChoice(typeof(IInterface), typeof(DecoratorA), typeof(DecoratorB))]
internal partial class Container
{
    [DecoratorSequenceChoice(typeof(IInterface), typeof(DecoratorA), typeof(DecoratorB))]
    [CustomScopeForRootTypes(typeof(ScopeRoot0))]
    private partial class DIE_Scope_0
    {
        
    }
    
    [DecoratorSequenceChoice(typeof(IInterface), typeof(DecoratorA))]
    [CustomScopeForRootTypes(typeof(ScopeRoot1))]
    private partial class DIE_Scope_1
    {
        
    }
}