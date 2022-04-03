using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Wrapped.TransientScopeInstanceFunctionAsValueTask;

internal class Dependency : ITaskTypeInitializer, ITransientScopeInstance
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

[CreateFunction(typeof(ValueTask<Dependency>), "Create")]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        using var container = new Container();
        var instance = container.Create();
        Assert.True((await instance.ConfigureAwait(false)).IsInitialized);
    }
}