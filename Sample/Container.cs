using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

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
        private sealed partial  class DIE_Scope_Sync;
        
        [CustomScopeForRootTypes(typeof(Async))]
        private sealed partial class DIE_Scope_Async;
    }
}