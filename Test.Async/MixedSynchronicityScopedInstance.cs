using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async;

public sealed partial class MixedSynchronicityScopedInstance
{
    internal sealed class Dependency : IScopeInstance, ITaskInitializer, IInitializer
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

    internal sealed class ScopeRootSync : IScopeRoot
    {
        internal required Dependency Dependency0 { get; init; }
        internal required Dependency Dependency1 { get; init; }
    }

    internal sealed class ScopeRootAsync : IScopeRoot
    {
        internal required Dependency Dependency0 { get; init; }
        internal required Dependency Dependency1 { get; init; }
    }

    internal sealed class Parent
    {
        internal required ScopeRootSync Sync { get; init; }
        internal required ValueTask<ScopeRootAsync> Async { get; init; }
    }

    [FilterInitializer(typeof(ITaskInitializer))]
    [FilterInitializer(typeof(IInitializer))]
    [CreateFunction(typeof(Parent), "Create")]
    internal sealed partial class Container
    {
        [Initializer(typeof(IInitializer), nameof(IInitializer.Initialize))]
        [CustomScopeForRootTypes(typeof(ScopeRootSync))]
        private sealed partial class DIE_Scope_Sync;
        
        [Initializer(typeof(ITaskInitializer), nameof(ITaskInitializer.InitializeAsync))]
        [CustomScopeForRootTypes(typeof(ScopeRootAsync))]
        private sealed partial class DIE_Scope_Async;
    }

    public sealed class Tests
    {
        [Fact]
        public async Task Test()
        {
            var container = MixedSynchronicityScopedInstance.Container.DIE_CreateContainer();
            var parent = container.Create();
            var sync = parent.Sync;
            var async = await parent.Async;
            Assert.True(sync.Dependency0.IsInitializedSync);
            Assert.True(async.Dependency0.IsInitializedAsync);
            Assert.True(sync.Dependency0 == sync.Dependency1);
            Assert.True(async.Dependency0 == async.Dependency1);
            Assert.True(sync.Dependency0 != async.Dependency0);
        }
    }
}