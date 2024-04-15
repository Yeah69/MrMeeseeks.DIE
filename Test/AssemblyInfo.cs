using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

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
[assembly:Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
[assembly:Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
[assembly:Initializer(typeof(IValueTaskInitializer), nameof(IValueTaskInitializer.InitializeAsync))]

[assembly:InjectionKeyMapping(typeof(InjectionKeyAttribute))]
[assembly:DecorationOrdinalMapping(typeof(DecorationOrdinalAttribute))]

[assembly:AllImplementationsAggregation]

[assembly:ErrorDescriptionInsteadOfBuildFailure]

//[assembly:Analytics(Analytics.ResolutionGraph)]

//[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]