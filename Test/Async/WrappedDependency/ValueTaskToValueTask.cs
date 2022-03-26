using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.WrappedDependency.ValueTaskToValueTask;

internal class Dependency : IValueTaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    async ValueTask IValueTaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
        IsInitialized = true;
    }
}

[CreateFunction(typeof(ValueTask<Dependency>), "Create0")]
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