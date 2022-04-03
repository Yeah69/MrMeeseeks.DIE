using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.Sample;
internal class Dependency : ITaskTypeInitializer, ITransientScopeInstance
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

[CreateFunction(typeof(Task<Dependency>), "Create")]
internal partial class Container
{
}