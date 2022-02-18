using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
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

[MultiContainer(typeof(Func<Task<Dependency>>))]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = new Container();
        var instance = container.Create0();
        Assert.True((await instance().ConfigureAwait(false)).IsInitialized);
    }
}