using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Sample;

//[assembly:Spy(typeof(IPublicTypeReport), typeof(IInternalTypeReport))]
[assembly:SingleInstanceAggregation(typeof(ISingleInstance))]
[assembly:ScopedInstanceAggregation(typeof(IScopedInstance))]
[assembly:ScopeRootAggregation(typeof(IScopeRoot))]
[assembly:TransientAggregation(typeof(ITransient))]
[assembly:DecoratorAggregation(typeof(IDecorator<>))]
[assembly:DecoratorSequenceChoice(typeof(IDecoratedMulti))]
[assembly:DecoratorSequenceChoice(typeof(DecoratorMultiBasisB), typeof(DecoratorMultiA), typeof(DecoratorMultiB))]