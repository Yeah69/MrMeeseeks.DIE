using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.Async.AwaitedDependency.Dependency;


internal class Dependency : ITaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

[CreateFunction(typeof(Dependency), "CreateDep")]
internal partial class Container
{
}