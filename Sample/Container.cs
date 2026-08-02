using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

public sealed partial class MixedSynchronicity
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

    internal sealed class ScopeRootSync : IScopeRoot
    {
        internal required Dependency Dependency { get; init; }
    }

    internal sealed class ScopeRootAsync : IScopeRoot
    {
        internal required Dependency Dependency { get; init; }
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
}
