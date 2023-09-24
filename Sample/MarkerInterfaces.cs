using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;

//[assembly:ErrorDescriptionInsteadOfBuildFailure]
[assembly:Analytics(Analytics.ResolutionGraph | Analytics.ErrorFilteredResolutionGraph)]

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

//[assembly:InjectionKeyMapping(typeof(KeyAttribute))]

[assembly:AllImplementationsAggregation]

namespace MrMeeseeks.DIE.Sample;

public interface IContainerInstance { }
public interface ITransientScopeInstance { }
public interface IScopeInstance { }
public interface ITransientScopeRoot { }
public interface IScopeRoot { }
public interface ITransient { }
public interface ISyncTransient { }
public interface IAsyncTransient { }
// ReSharper disable once UnusedTypeParameter
public interface IDecorator<T> { }
// ReSharper disable once UnusedTypeParameter
public interface IComposite<T> { }
public interface IInitializer
{
    void Initialize();
}
public interface ITaskInitializer
{
    Task InitializeAsync();
}
public interface IValueTaskInitializer
{
    ValueTask InitializeAsync();
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public class KeyAttribute : Attribute
{
    public KeyAttribute(object key) => Key = key;

    public object Key { get; }
}