using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.MsContainer;

//[assembly:ErrorDescriptionInsteadOfBuildFailure]

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

[assembly:AllImplementationsAggregation]

namespace MrMeeseeks.DIE.MsContainer;

internal interface IContainerInstance;
internal interface ITransientScopeInstance;
internal interface IScopeInstance;
internal interface ITransientScopeRoot;
internal interface IScopeRoot;
internal interface ITransient;
internal interface ISyncTransient;
internal interface IAsyncTransient;
// ReSharper disable once UnusedTypeParameter
internal interface IDecorator<T>;
// ReSharper disable once UnusedTypeParameter
internal interface IComposite<T>;
internal interface IInitializer
{
    void Initialize();
}
internal interface ITaskInitializer
{
    Task InitializeAsync();
}