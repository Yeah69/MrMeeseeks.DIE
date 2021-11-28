using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Sample;

//[assembly:Spy(typeof(IPublicTypeReport), typeof(IInternalTypeReport))]
[assembly:SingleInstance(typeof(ISingleInstance))]
[assembly:ScopedInstance(typeof(IScopedInstance))]
[assembly:ScopeRoot(typeof(IScopeRoot))]
[assembly:Transient(typeof(ITransient))]
[assembly:Decorator(typeof(IDecorator<>))]