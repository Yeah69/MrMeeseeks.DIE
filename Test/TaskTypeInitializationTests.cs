using System;
using System.Threading.Tasks;
using Xunit;

namespace MrMeeseeks.DIE.Test.TaskTypeInitializationTests;

internal class Dependency : ITaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
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