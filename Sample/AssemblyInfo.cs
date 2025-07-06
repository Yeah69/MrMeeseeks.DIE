using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;
using MrMeeseeks.DIE.UserUtility;

//[assembly:ErrorDescriptionInsteadOfBuildFailure]
//[assembly:Analytics(Analytics.ResolutionGraph | Analytics.ErrorFilteredResolutionGraph)]

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

[assembly:InvocationDescriptionMapping(typeof(InvocationDescriptionAttribute))]

[assembly:AllImplementationsAggregation]

[assembly:ConstructorChoice(typeof(Lazy<>), typeof(Func<>))]
[assembly:ConstructorChoice(typeof(ThreadLocal<>), typeof(Func<>))]
[assembly:ConstructorChoice(typeof(ConcurrentBag<>), typeof(IEnumerable<>))]
[assembly:ConstructorChoice(typeof(ConcurrentQueue<>), typeof(IEnumerable<>))]
[assembly:ConstructorChoice(typeof(ConcurrentStack<>), typeof(IEnumerable<>))]
[assembly:ConstructorChoice(typeof(HashSet<>), typeof(IEnumerable<>))]
[assembly:ConstructorChoice(typeof(LinkedList<>), typeof(IEnumerable<>))]
[assembly:ConstructorChoice(typeof(List<>), typeof(IEnumerable<>))]
[assembly:ConstructorChoice(typeof(Queue<>), typeof(IEnumerable<>))]
[assembly:ConstructorChoice(typeof(SortedSet<>), typeof(IEnumerable<>))]
[assembly:ConstructorChoice(typeof(Stack<>), typeof(IEnumerable<>))]
[assembly:ConstructorChoice(typeof(Collection<>), typeof(IList<>))]
[assembly:ImplementationChoice(typeof(IList<>), typeof(List<>))]
[assembly:ImplementationChoice(typeof(IReadOnlyCollection<>), typeof(ReadOnlyCollection<>))]
[assembly:ImplementationChoice(typeof(IReadOnlyList<>), typeof(ReadOnlyCollection<>))]
[assembly:ImplementationChoice(typeof(ICollection<>), typeof(Collection<>))]

namespace MrMeeseeks.DIE.Sample;

[AttributeUsage(AttributeTargets.Interface)]
internal sealed class InvocationDescriptionAttribute : Attribute;

