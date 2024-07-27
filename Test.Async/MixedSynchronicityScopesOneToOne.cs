using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async;

public sealed partial class MixedSynchronicityScopesOneToOne
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

    internal sealed class ScopeRootSyncSync : IScopeRoot
    {
        internal required SyncDependency Dependency { get; init; }
    }

    internal sealed class ScopeRootAsyncAsync : IScopeRoot
    {
        internal required AsyncDependency Dependency { get; init; }
    }

    internal sealed class ScopeRootSyncAsync : IScopeRoot
    {
        internal required SyncDependency Dependency { get; init; }
    }

    internal sealed class ScopeRootAsyncSync : IScopeRoot
    {
        internal required AsyncDependency Dependency { get; init; }
    }

    internal sealed class Parent
    {
        internal required ScopeRootSyncSync SyncSync { get; init; }
        internal required ValueTask<ScopeRootAsyncAsync> AsyncAsync { get; init; }
        internal required ValueTask<ScopeRootSyncAsync> SyncAsync { get; init; }
        internal required ValueTask<ScopeRootAsyncSync> AsyncSync { get; init; }
    }

    [CreateFunction(typeof(Parent), "Create")]
    internal sealed partial class Container
    {
        [CustomScopeForRootTypes(typeof(ScopeRootSyncSync))]
        private sealed partial class DIE_Scope_SyncSync
        {
            internal DIE_Scope_SyncSync(SyncDependency syncDependency){}
        }

        [CustomScopeForRootTypes(typeof(ScopeRootAsyncAsync))]
        private sealed partial class DIE_Scope_AsyncAsync
        {
            internal DIE_Scope_AsyncAsync(AsyncDependency asyncDependency){}
        }
        [CustomScopeForRootTypes(typeof(ScopeRootAsyncSync))]
        private sealed partial class DIE_Scope_AsyncSync
        {
            internal DIE_Scope_AsyncSync(SyncDependency syncDependency){}
        }

        [CustomScopeForRootTypes(typeof(ScopeRootSyncAsync))]
        private sealed partial class DIE_Scope_SyncAsync
        {
            internal DIE_Scope_SyncAsync(AsyncDependency asyncDependency){}
        }
    }

    public sealed class Tests
    {
        [Fact]
        public async Task Test()
        {
            var container = Container.DIE_CreateContainer();
            var parent = container.Create();
            var syncSync = parent.SyncSync;
            var asyncAsync = await parent.AsyncAsync;
            var syncAsync = await parent.SyncAsync;
            var asyncSync = await parent.AsyncSync;
            Assert.True(syncSync.Dependency.IsInitialized);
            Assert.True(asyncAsync.Dependency.IsInitialized);
            Assert.True(syncAsync.Dependency.IsInitialized);
            Assert.True(asyncSync.Dependency.IsInitialized);
        }
    }
}