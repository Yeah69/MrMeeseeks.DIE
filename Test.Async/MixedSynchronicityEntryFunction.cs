using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async;

public sealed partial class MixedSynchronicityEntryFunction
{
    internal sealed class Dependency : ITaskInitializer
    {
        public bool IsInitialized { get; private set; }

        async Task ITaskInitializer.InitializeAsync()
        {
            await Task.Delay(500);
            IsInitialized = true;
        }
    }

    internal sealed class Sync : IScopeRoot
    {
        internal required Dependency Dependency { get; init; }
    }

    internal sealed class Async : IScopeRoot
    {
        internal required Dependency Dependency { get; init; }
    }

    [CreateFunction(typeof(Sync), "CreateSync")]
    [CreateFunction(typeof(ValueTask<Async>), "CreateAsync")]
    internal sealed partial class Container
    {
        [FilterInitializer(typeof(ITaskInitializer))]
        [CustomScopeForRootTypes(typeof(Sync))]
        private sealed partial class DIE_Scope_Sync;

        [CustomScopeForRootTypes(typeof(Async))]
        private sealed partial class DIE_Scope_Async;
    }

    public sealed class Tests
    {
        [Fact]
        public async Task Test()
        {
            var container = Container.DIE_CreateContainer();
            var sync = container.CreateSync().Dependency;
            var async = (await container.CreateAsync()).Dependency;
            Assert.False(sync.IsInitialized);
            Assert.True(async.IsInitialized);
            Assert.NotEqual(sync, async);
        }
    }
}