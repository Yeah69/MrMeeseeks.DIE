using System;
using System.Threading.Tasks;
using Xunit;

namespace MrMeeseeks.DIE.Test.ValueTaskTypeInitializationTests;

internal class Dependency : IValueTaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    async ValueTask IValueTaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal partial class Container : IContainer<Dependency>
{
}

public partial class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = ((IContainer<Dependency>) container).Resolve();
        Assert.True(instance.IsInitialized);
    }
}