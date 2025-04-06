using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal sealed class Dependency : ITaskInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskInitializer.InitializeAsync()
    {
        await Task.Delay(10000);
        IsInitialized = true;
    }
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container;