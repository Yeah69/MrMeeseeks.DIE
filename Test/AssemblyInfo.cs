using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Test;

[assembly:ContainerInstanceAggregation(typeof(IContainerInstance))]
[assembly:TransientScopeInstanceAggregation(typeof(ITransientScopeInstance))]
[assembly:ScopeInstanceAggregation(typeof(IScopeInstance))]
[assembly:TransientScopeRootAggregation(typeof(ITransientScopeRoot))]
[assembly:ScopeRootAggregation(typeof(IScopeRoot))]
[assembly:TransientAggregation(typeof(ITransient))]
[assembly:SyncTransientAggregation(typeof(ISyncTransient))]
[assembly:AsyncTransientAggregation(typeof(IAsyncTransient))]
[assembly:DecoratorAggregation(typeof(IDecorator<>))]
[assembly:CompositeAggregation(typeof(IComposite<>))]
[assembly:TypeInitializer(typeof(ITypeInitializer), nameof(ITypeInitializer.Initialize))]
[assembly:TypeInitializer(typeof(ITaskTypeInitializer), nameof(ITaskTypeInitializer.InitializeAsync))]
[assembly:TypeInitializer(typeof(IValueTaskTypeInitializer), nameof(IValueTaskTypeInitializer.InitializeAsync))]

[assembly:AllImplementationsAggregation]

[assembly:ErrorDescriptionInsteadOfBuildFailure]