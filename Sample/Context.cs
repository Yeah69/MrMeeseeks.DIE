using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.Sample;
internal class Dependency : ITaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class Instance : ITransientScopeInstance
{
    public Dependency Dependency { get; }

    internal Instance(Dependency dependency) => Dependency = dependency;
}



[CreateFunction(typeof(Func<Dependency, Task<Instance>>), "CreateWithParameter")]
[CreateFunction(typeof(Func<Task<Instance>>), "CreateWithoutParameter")]
internal partial class Container
{
}