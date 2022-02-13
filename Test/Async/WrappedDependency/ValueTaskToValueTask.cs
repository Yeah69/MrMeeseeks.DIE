using System;
using System.Threading.Tasks;
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

internal partial class Container : IContainer<ValueTask<Dependency>>
{
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = new Container();
        var instance = await ((IContainer<ValueTask<Dependency>>) container).Resolve().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}