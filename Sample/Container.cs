using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

public sealed partial class MixedSynchronicityScopedInstanceMore
{
    internal sealed class Dependency : ITaskInitializer, IInitializer
    {
        public bool IsInitializedAsync { get; private set; }
        public bool IsInitializedSync { get; private set; }
    
        async Task ITaskInitializer.InitializeAsync()
        {
            await Task.Delay(500);
            IsInitializedAsync = true;
        }
    
        void IInitializer.Initialize() => 
            IsInitializedSync = true;
    }

    internal abstract class RootBase
    {
        internal required Dependency Dependency0 { get; init; }
        internal required Dependency Dependency1 { get; init; }
    }

    internal abstract class ScopeRootBase : RootBase, IScopeRoot;

    internal sealed class ScopeRootSync_Container : ScopeRootBase;
    internal sealed class ScopeRootAsync_Container : ScopeRootBase;
    internal sealed class ScopeRootSync_TransientScope : ScopeRootBase;
    internal sealed class ScopeRootAsync_TransientScope : ScopeRootBase;
    internal sealed class ScopeRootSync_Scope : ScopeRootBase;
    internal sealed class ScopeRootAsync_Scope : ScopeRootBase;
    internal sealed class ScopeRootSync_Transient : ScopeRootBase;
    internal sealed class ScopeRootAsync_Transient : ScopeRootBase;

    internal abstract class TransientScopeRootBase : RootBase, IScopeRoot;

    internal sealed class TransientScopeRootSync_Container : TransientScopeRootBase;
    internal sealed class TransientScopeRootAsync_Container : TransientScopeRootBase;
    internal sealed class TransientScopeRootSync_TransientScope : TransientScopeRootBase;
    internal sealed class TransientScopeRootAsync_TransientScope : TransientScopeRootBase;
    internal sealed class TransientScopeRootSync_Scope : TransientScopeRootBase;
    internal sealed class TransientScopeRootAsync_Scope : TransientScopeRootBase;
    internal sealed class TransientScopeRootSync_Transient : TransientScopeRootBase;
    internal sealed class TransientScopeRootAsync_Transient : TransientScopeRootBase;

    internal sealed class Parent
    {
        internal required ScopeRootSync_Container Sync_Container { get; init; }
        internal required ValueTask<ScopeRootAsync_Container> Async_Container { get; init; }
        internal required ScopeRootSync_TransientScope Sync_TransientScope { get; init; }
        internal required ValueTask<ScopeRootAsync_TransientScope> Async_TransientScope { get; init; }
        internal required ScopeRootSync_Scope Sync_Scope { get; init; }
        internal required ValueTask<ScopeRootAsync_Scope> Async_Scope { get; init; }
        internal required ScopeRootSync_Transient Sync_Transient { get; init; }
        internal required ValueTask<ScopeRootAsync_Transient> Async_Transient { get; init; }
        
        internal required TransientScopeRootSync_Container Transient_Sync_Container { get; init; }
        internal required ValueTask<TransientScopeRootAsync_Container> Transient_Async_Container { get; init; }
        internal required TransientScopeRootSync_TransientScope Transient_Sync_TransientScope { get; init; }
        internal required ValueTask<TransientScopeRootAsync_TransientScope> Transient_Async_TransientScope { get; init; }
        internal required TransientScopeRootSync_Scope Transient_Sync_Scope { get; init; }
        internal required ValueTask<TransientScopeRootAsync_Scope> Transient_Async_Scope { get; init; }
        internal required TransientScopeRootSync_Transient Transient_Sync_Transient { get; init; }
        internal required ValueTask<TransientScopeRootAsync_Transient> Transient_Async_Transient { get; init; }
    }

    [FilterInitializer(typeof(ITaskInitializer))]
    [FilterInitializer(typeof(IInitializer))]
    [CreateFunction(typeof(Parent), "Create")]
    internal sealed partial class Container
    {
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [ContainerInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootSync_Container))]
        private sealed partial class DIE_Scope_Sync_Container;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [ContainerInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootAsync_Container))]
        private sealed partial class DIE_Scope_Async_Container;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [TransientScopeInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootSync_TransientScope))]
        private sealed partial class DIE_Scope_Sync_TransientScope;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [TransientScopeInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootAsync_TransientScope))]
        private sealed partial class DIE_Scope_Async_TransientScope;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [ScopeInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootSync_Scope))]
        private sealed partial class DIE_Scope_Sync_Scope;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [ScopeInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootAsync_Scope))]
        private sealed partial class DIE_Scope_Async_Scope;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [CustomScopeForRootTypes(typeof(ScopeRootSync_Transient))]
        private sealed partial class DIE_Scope_Sync_Transient;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [CustomScopeForRootTypes(typeof(ScopeRootAsync_Transient))]
        private sealed partial class DIE_Scope_Async_Transient;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [ContainerInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootSync_Container))]
        private sealed partial class DIE_TransientScope_Sync_Container;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [ContainerInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootAsync_Container))]
        private sealed partial class DIE_TransientScope_Async_Container;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [TransientScopeInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootSync_TransientScope))]
        private sealed partial class DIE_TransientScope_Sync_TransientScope;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [TransientScopeInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootAsync_TransientScope))]
        private sealed partial class DIE_TransientScope_Async_TransientScope;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [ScopeInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootSync_Scope))]
        private sealed partial class DIE_TransientScope_Sync_Scope;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [ScopeInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootAsync_Scope))]
        private sealed partial class DIE_TransientScope_Async_Scope;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootSync_Transient))]
        private sealed partial class DIE_TransientScope_Sync_Transient;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootAsync_Transient))]
        private sealed partial class DIE_TransientScope_Async_Transient;
    }
}

/*public sealed partial class MixedSynchronicityScopedInstanceMore
{
    internal sealed class Dependency : ITaskInitializer, IInitializer
    {
        public bool IsInitializedAsync { get; private set; }
        public bool IsInitializedSync { get; private set; }
    
        async Task ITaskInitializer.InitializeAsync()
        {
            await Task.Delay(500);
            IsInitializedAsync = true;
        }
    
        void IInitializer.Initialize() => 
            IsInitializedSync = true;
    }

    internal abstract class ScopeRootBase : IScopeRoot
    {
        internal required Dependency Dependency0 { get; init; }
        internal required Dependency Dependency1 { get; init; }
    }

    internal sealed class ScopeRootSync_Container : ScopeRootBase;
    internal sealed class ScopeRootAsync_Container : ScopeRootBase;
    internal sealed class ScopeRootSync_TransientScope : ScopeRootBase;
    internal sealed class ScopeRootAsync_TransientScope : ScopeRootBase;
    internal sealed class ScopeRootSync_Scope : ScopeRootBase;
    internal sealed class ScopeRootAsync_Scope : ScopeRootBase;
    internal sealed class ScopeRootSync_Transient : ScopeRootBase;
    internal sealed class ScopeRootAsync_Transient : ScopeRootBase;

    internal sealed class Parent
    {
        internal required ScopeRootSync_Container Sync_Container { get; init; }
        internal required ValueTask<ScopeRootAsync_Container> Async_Container { get; init; }
        internal required ScopeRootSync_TransientScope Sync_TransientScope { get; init; }
        internal required ValueTask<ScopeRootAsync_TransientScope> Async_TransientScope { get; init; }
        internal required ScopeRootSync_Scope Sync_Scope { get; init; }
        internal required ValueTask<ScopeRootAsync_Scope> Async_Scope { get; init; }
        internal required ScopeRootSync_Transient Sync_Transient { get; init; }
        internal required ValueTask<ScopeRootAsync_Transient> Async_Transient { get; init; }
    }

    [FilterInitializer(typeof(ITaskInitializer))]
    [FilterInitializer(typeof(IInitializer))]
    [CreateFunction(typeof(Parent), "Create")]
    internal sealed partial class Container
    {
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [ContainerInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootSync_Container))]
        private sealed partial class DIE_Scope_Sync_Container;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [ContainerInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootAsync_Container))]
        private sealed partial class DIE_Scope_Async_Container;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [TransientScopeInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootSync_TransientScope))]
        private sealed partial class DIE_Scope_Sync_TransientScope;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [TransientScopeInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootAsync_TransientScope))]
        private sealed partial class DIE_Scope_Async_TransientScope;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [ScopeInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootSync_Scope))]
        private sealed partial class DIE_Scope_Sync_Scope;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [ScopeInstanceImplementationAggregation(typeof(Dependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootAsync_Scope))]
        private sealed partial class DIE_Scope_Async_Scope;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [CustomScopeForRootTypes(typeof(ScopeRootSync_Transient))]
        private sealed partial class DIE_Scope_Sync_Transient;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [CustomScopeForRootTypes(typeof(ScopeRootAsync_Transient))]
        private sealed partial class DIE_Scope_Async_Transient;
    }
}*/
