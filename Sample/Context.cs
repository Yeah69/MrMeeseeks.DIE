using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.Async.TaskCollection;

internal interface IInterface
{
    bool IsInitialized { get; }
}

internal class DependencyA : ITaskTypeInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class DependencyB : IValueTaskTypeInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    async ValueTask IValueTaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class DependencyC : ITypeInitializer, IInterface
{
    public bool IsInitialized { get; private set; }
    
    void ITypeInitializer.Initialize()
    {
        IsInitialized = true;
    }
}

internal class DependencyD : IInterface
{
    public bool IsInitialized => true;
}

[MultiContainer(typeof(IReadOnlyList<Task<IInterface>>))]
internal partial class Container
{
}