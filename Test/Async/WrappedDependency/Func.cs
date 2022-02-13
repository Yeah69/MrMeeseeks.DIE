using System;
using System.Threading.Tasks;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.WrappedDependency.Func;


internal class Dependency : ITaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal partial class Container : IContainer<Func<Task<Dependency>>>
{
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = new Container();
        var instance = ((IContainer<Func<Task<Dependency>>>) container).Resolve();
        Assert.True((await instance().ConfigureAwait(false)).IsInitialized);
    }
}