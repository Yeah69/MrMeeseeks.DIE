using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Wrapped.TaskToTask;

internal class Dependency : ITaskInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskInitializer.InitializeAsync()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
        IsInitialized = true;
    }
}
[CreateFunction(typeof(Task<Dependency>), "Create")]
internal sealed partial class Container
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = await container.Create().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}