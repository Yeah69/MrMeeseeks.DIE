using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Sample;

[assembly:SingleInstanceAggregation(typeof(ISingleInstance))]
[assembly:ScopedInstanceAggregation(typeof(IScopedInstance))]
[assembly:ScopeRootAggregation(typeof(IScopeRoot))]
[assembly:TransientAggregation(typeof(ITransient))]
[assembly:DecoratorAggregation(typeof(IDecorator<>))]
[assembly:CompositeAggregation(typeof(IComposite<>))]