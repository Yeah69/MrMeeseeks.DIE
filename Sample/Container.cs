using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;

[assembly:ContainerInstanceAbstractionAggregation(typeof(IContainerInstance))]
[assembly:TransientScopeInstanceAbstractionAggregation(typeof(ITransientScopeInstance))]
[assembly:ScopeInstanceAbstractionAggregation(typeof(IScopeInstance))]
[assembly:TransientScopeRootAbstractionAggregation(typeof(ITransientScopeRoot))]
[assembly:ScopeRootAbstractionAggregation(typeof(IScopeRoot))]
[assembly:TransientAbstractionAggregation(typeof(ITransient))]
[assembly:SyncTransientAbstractionAggregation(typeof(ISyncTransient))]
[assembly:AsyncTransientAbstractionAggregation(typeof(IAsyncTransient))]
[assembly:DecoratorAbstractionAggregation(typeof(IDecorator<>))]
[assembly:CompositeAbstractionAggregation(typeof(IComposite<>))]
[assembly:TypeInitializer(typeof(ITypeInitializer), nameof(ITypeInitializer.Initialize))]
[assembly:TypeInitializer(typeof(ITaskTypeInitializer), nameof(ITaskTypeInitializer.InitializeAsync))]
[assembly:TypeInitializer(typeof(IValueTaskTypeInitializer), nameof(IValueTaskTypeInitializer.InitializeAsync))]

[assembly:AllImplementationsAggregation]

//[assembly:ErrorDescriptionInsteadOfBuildFailure]