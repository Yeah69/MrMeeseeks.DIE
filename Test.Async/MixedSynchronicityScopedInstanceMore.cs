using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async;

public sealed partial class MixedSynchronicityScopedInstanceMore
{
    internal sealed class SyncDependency : IInitializer
    {
        public bool IsInitialized { get; private set; }
    
        void IInitializer.Initialize() => 
            IsInitialized = true;
    }
    internal sealed class AsyncDependency : ITaskInitializer
    {
        public bool IsInitialized { get; private set; }
    
        async Task ITaskInitializer.InitializeAsync()
        {
            await Task.Delay(500);
            IsInitialized = true;
        }
    }

    internal abstract class SyncRootBase
    {
        internal required SyncDependency Dependency0 { get; init; }
        internal required SyncDependency Dependency1 { get; init; }
    }

    internal abstract class AsyncRootBase
    {
        internal required AsyncDependency Dependency0 { get; init; }
        internal required AsyncDependency Dependency1 { get; init; }
    }

    internal abstract class SyncScopeRootBase : SyncRootBase, IScopeRoot;
    internal abstract class AsyncScopeRootBase : AsyncRootBase, IScopeRoot;

    internal sealed class ScopeRootSync_Container : SyncScopeRootBase;
    internal sealed class ScopeRootAsync_Container : AsyncScopeRootBase;
    internal sealed class ScopeRootSync_TransientScope : SyncScopeRootBase;
    internal sealed class ScopeRootAsync_TransientScope : AsyncScopeRootBase;
    internal sealed class ScopeRootSync_Scope : SyncScopeRootBase;
    internal sealed class ScopeRootAsync_Scope : AsyncScopeRootBase;
    internal sealed class ScopeRootSync_Transient : SyncScopeRootBase;
    internal sealed class ScopeRootAsync_Transient : AsyncScopeRootBase;


    internal abstract class SyncTransientScopeRootBase : SyncRootBase, ITransientScopeRoot;
    internal abstract class AsyncTransientScopeRootBase : AsyncRootBase, ITransientScopeRoot;

    internal sealed class TransientScopeRootSync_Container : SyncTransientScopeRootBase;
    internal sealed class TransientScopeRootAsync_Container : AsyncTransientScopeRootBase;
    internal sealed class TransientScopeRootSync_TransientScope : SyncTransientScopeRootBase;
    internal sealed class TransientScopeRootAsync_TransientScope : AsyncTransientScopeRootBase;
    internal sealed class TransientScopeRootSync_Scope : SyncTransientScopeRootBase;
    internal sealed class TransientScopeRootAsync_Scope : AsyncTransientScopeRootBase;
    internal sealed class TransientScopeRootSync_Transient : SyncTransientScopeRootBase;
    internal sealed class TransientScopeRootAsync_Transient : AsyncTransientScopeRootBase;

    internal sealed class Parent
    {
        internal required ScopeRootSync_Container Sync_Container { get; init; }
        internal required ValueTask<ScopeRootAsync_Container> Async_Container { get; init; } 
        internal required ScopeRootSync_TransientScope Sync_TransientScope { get; init; }
        //internal required Task<ScopeRootAsync_TransientScope> Async_TransientScope { get; init; } // Todo check why this leads to deadlock
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
        [ContainerInstanceImplementationAggregation(typeof(SyncDependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootSync_Container))]
        private sealed partial class DIE_Scope_Sync_Container;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [ContainerInstanceImplementationAggregation(typeof(AsyncDependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootAsync_Container))]
        private sealed partial class DIE_Scope_Async_Container;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [TransientScopeInstanceImplementationAggregation(typeof(SyncDependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootSync_TransientScope))]
        private sealed partial class DIE_Scope_Sync_TransientScope;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [TransientScopeInstanceImplementationAggregation(typeof(AsyncDependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootAsync_TransientScope))]
        private sealed partial class DIE_Scope_Async_TransientScope;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [ScopeInstanceImplementationAggregation(typeof(SyncDependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootSync_Scope))]
        private sealed partial class DIE_Scope_Sync_Scope;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [ScopeInstanceImplementationAggregation(typeof(AsyncDependency))]
        [CustomScopeForRootTypes(typeof(ScopeRootAsync_Scope))]
        private sealed partial class DIE_Scope_Async_Scope;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [CustomScopeForRootTypes(typeof(ScopeRootSync_Transient))]
        private sealed partial class DIE_Scope_Sync_Transient;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [CustomScopeForRootTypes(typeof(ScopeRootAsync_Transient))]
        private sealed partial class DIE_Scope_Async_Transient;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [ContainerInstanceImplementationAggregation(typeof(SyncDependency))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootSync_Container))]
        private sealed partial class DIE_TransientScope_Sync_Container;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [ContainerInstanceImplementationAggregation(typeof(AsyncDependency))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootAsync_Container))]
        private sealed partial class DIE_TransientScope_Async_Container;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [TransientScopeInstanceImplementationAggregation(typeof(SyncDependency))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootSync_TransientScope))]
        private sealed partial class DIE_TransientScope_Sync_TransientScope;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [TransientScopeInstanceImplementationAggregation(typeof(AsyncDependency))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootAsync_TransientScope))]
        private sealed partial class DIE_TransientScope_Async_TransientScope;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [ScopeInstanceImplementationAggregation(typeof(SyncDependency))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootSync_Scope))]
        private sealed partial class DIE_TransientScope_Sync_Scope;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [ScopeInstanceImplementationAggregation(typeof(AsyncDependency))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootAsync_Scope))]
        private sealed partial class DIE_TransientScope_Async_Scope;
        
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootSync_Transient))]
        private sealed partial class DIE_TransientScope_Sync_Transient;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [CustomScopeForRootTypes(typeof(TransientScopeRootAsync_Transient))]
        private sealed partial class DIE_TransientScope_Async_Transient;
    }

    public sealed class Tests
    {
        [Fact]
        public async Task Test()
        {
            var container = Container.DIE_CreateContainer();
            var parent = container.Create();
            
            var sync_container = parent.Sync_Container;
            var async_container = await parent.Async_Container;
            var sync_transientscope = parent.Sync_TransientScope;
            //var async_transientscope = await parent.Async_TransientScope;
            var sync_scope = parent.Sync_Scope;
            var async_scope = await parent.Async_Scope;
            var sync_transient = parent.Sync_Transient;
            var async_transient = await parent.Async_Transient;
            
            Assert.Same(sync_container.Dependency0, sync_container.Dependency1);
            Assert.Same(async_container.Dependency0, async_container.Dependency1);
            
            Assert.Same(sync_transientscope.Dependency0, sync_transientscope.Dependency1);
            //Assert.Same(async_transientscope.Dependency0, async_transientscope.Dependency1);
            
            //Assert.Same(sync_container.Dependency0, sync_transientscope.Dependency0);
            
            Assert.Same(sync_scope.Dependency0, sync_scope.Dependency1);
            Assert.True(sync_scope.Dependency0.IsInitialized);
            Assert.Same(async_scope.Dependency0, async_scope.Dependency1);
            Assert.True(async_scope.Dependency0.IsInitialized);
            
            Assert.NotSame(sync_transient.Dependency0, sync_transient.Dependency1);
            Assert.True(sync_transient.Dependency0.IsInitialized);
            Assert.NotSame(async_transient.Dependency0, async_transient.Dependency1);
            Assert.True(async_transient.Dependency0.IsInitialized);
            
            var transient_sync_container = parent.Transient_Sync_Container;
            var transient_async_container = await parent.Transient_Async_Container;
            var transient_sync_transientscope = parent.Transient_Sync_TransientScope;
            var transient_async_transientscope = await parent.Transient_Async_TransientScope;
            var transient_sync_scope = parent.Transient_Sync_Scope;
            var transient_async_scope = await parent.Transient_Async_Scope;
            var transient_sync_transient = parent.Transient_Sync_Transient;
            var transient_async_transient = await parent.Transient_Async_Transient;
            
            Assert.Same(transient_sync_container.Dependency0, transient_sync_container.Dependency1);
            Assert.Same(transient_async_container.Dependency0, transient_async_container.Dependency1);
            
            Assert.Same(transient_sync_transientscope.Dependency0, transient_sync_transientscope.Dependency1);
            Assert.True(transient_sync_transientscope.Dependency0.IsInitialized);
            Assert.Same(transient_async_transientscope.Dependency0, transient_async_transientscope.Dependency1);
            Assert.True(transient_async_transientscope.Dependency0.IsInitialized);
            
            Assert.NotSame(transient_sync_container.Dependency0, transient_sync_transientscope.Dependency0);
            
            Assert.Same(transient_sync_scope.Dependency0, transient_sync_scope.Dependency1);
            Assert.True(transient_sync_scope.Dependency0.IsInitialized);
            Assert.Same(transient_async_scope.Dependency0, transient_async_scope.Dependency1);
            Assert.True(transient_async_scope.Dependency0.IsInitialized);
            
            Assert.NotSame(transient_sync_transient.Dependency0, transient_sync_transient.Dependency1);
            Assert.True(transient_sync_transient.Dependency0.IsInitialized);
            Assert.NotSame(transient_async_transient.Dependency0, transient_async_transient.Dependency1);
            Assert.True(transient_async_transient.Dependency0.IsInitialized);
        }
    }
}