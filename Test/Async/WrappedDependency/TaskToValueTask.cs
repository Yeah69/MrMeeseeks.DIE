using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.WrappedDependency.TaskToValueTask;

internal class Dependency : ITaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
        IsInitialized = true;
    }
}

[MultiContainer(typeof(ValueTask<Dependency>))]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = new Container();
        var instance = await container.Create0().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}