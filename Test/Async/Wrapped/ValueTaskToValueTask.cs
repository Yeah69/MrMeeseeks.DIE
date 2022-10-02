using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Wrapped.ValueTaskToValueTask;

internal class Dependency : IValueTaskInitializer
{
    public bool IsInitialized { get; private set; }
    
    async ValueTask IValueTaskInitializer.InitializeAsync()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
        IsInitialized = true;
    }
}

[CreateFunction(typeof(ValueTask<Dependency>), "Create")]
internal sealed partial class Container
{
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = new Container();
        var instance = await container.Create().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}