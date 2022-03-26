using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
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

[CreateFunction(typeof(Lazy<Task<Dependency>>), "CreateDep")]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = new Container();
        var instance = container.CreateDep();
        Assert.True((await instance.Value.ConfigureAwait(false)).IsInitialized);
    }
}