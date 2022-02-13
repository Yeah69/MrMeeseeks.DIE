using System;
using System.Threading.Tasks;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.WrappedDependency.Lazy;


internal class Dependency : ITaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal partial class Container : IContainer<Lazy<Task<Dependency>>>
{
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = new Container();
        var instance = ((IContainer<Lazy<Task<Dependency>>>) container).Resolve();
        Assert.True((await instance.Value.ConfigureAwait(false)).IsInitialized);
    }
}