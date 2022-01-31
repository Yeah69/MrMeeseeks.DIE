﻿using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.ScopeSpecificAttributesTestsWithDecorator;

internal interface IInterface
{
    int CheckNumber { get; }
}

internal class Implementation : IInterface
{
    public int CheckNumber => 1;
}

internal class ContainerDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 69;
    
    internal ContainerDecorator(IInterface _) {}
}

internal class ScopeDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 3;
    
    internal ScopeDecorator(IInterface _) {}
}

internal class TransientScopeDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 23;
    
    internal TransientScopeDecorator(IInterface _) {}
}

internal class TransientScopeSpecificDecorator : IInterface, IDecorator<IInterface>
{
    public int CheckNumber => 7;
    
    internal TransientScopeSpecificDecorator(IInterface _) {}
}

internal class TransientScopeRoot : ITransientScopeRoot
{
    public IInterface Dep { get; }
    
    internal TransientScopeRoot(IInterface dep)
    {
        Dep = dep;
    }
}

internal class TransientScopeRootSpecific : ITransientScopeRoot
{
    public IInterface Dep { get; }
    
    internal TransientScopeRootSpecific(IInterface dep)
    {
        Dep = dep;
    }
}

internal class ScopeRoot : IScopeRoot
{
    public IInterface Dep { get; }
    
    internal ScopeRoot(IInterface dep)
    {
        Dep = dep;
    }
}


[DecoratorSequenceChoice(typeof(IInterface), typeof(ContainerDecorator))]
internal partial class Container 
    : IContainer<IInterface>, 
        IContainer<TransientScopeRoot>, 
        IContainer<TransientScopeRootSpecific>, 
        IContainer<ScopeRoot>
{
    [DecoratorSequenceChoice(typeof(IInterface), typeof(TransientScopeDecorator))]
    internal partial class DIE_DefaultTransientScope
    {
        
    }
    [DecoratorSequenceChoice(typeof(IInterface), typeof(TransientScopeSpecificDecorator))]
    [CustomScopeForRootTypes(typeof(TransientScopeRootSpecific))]
    internal partial class DIE_TransientScope_A
    {
        
    }

    [DecoratorSequenceChoice(typeof(IInterface), typeof(ScopeDecorator))]
    internal partial class DIE_DefaultScope
    {
        
    }
}