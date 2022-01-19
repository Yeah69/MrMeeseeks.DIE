using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Sample;

[assembly:ContainerInstanceAggregation(typeof(IContainerInstance))]
[assembly:TransientScopeInstanceAggregation(typeof(ITransientScopeInstance))]
[assembly:ScopeInstanceAggregation(typeof(IScopeInstance))]
[assembly:TransientScopeRootAggregation(typeof(ITransientScopeRoot))]
[assembly:ScopeRootAggregation(typeof(IScopeRoot))]
[assembly:TransientAggregation(typeof(ITransient))]
[assembly:DecoratorAggregation(typeof(IDecorator<>))]
[assembly:CompositeAggregation(typeof(IComposite<>))]