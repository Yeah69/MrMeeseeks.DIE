using System.Threading.Tasks;

namespace MrMeeseeks.DIE.Sample;
internal class Dependency : ITaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    Task ITaskTypeInitializer.InitializeAsync()
    {
        IsInitialized = true;
        return Task.CompletedTask;
    }
}

internal partial class Container 
    : IContainer<Dependency>
{
}