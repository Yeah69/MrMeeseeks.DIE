using System;
using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Test;

[assembly:SingleInstanceAggregation(typeof(ISingleInstance))]
[assembly:ScopedInstanceAggregation(typeof(IScopedInstance))]
[assembly:ScopeRootAggregation(typeof(IScopeRoot))]
[assembly:TransientAggregation(typeof(ITransient))]
[assembly:DecoratorAggregation(typeof(IDecorator<>))]
[assembly:CompositeAggregation(typeof(IComposite<>))]

namespace MrMeeseeks.DIE.Test;

public class AssemblyInfo
{
    
}