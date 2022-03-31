using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Awaited.TaskTypeInitializerTask;


internal class Dependency : ITaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

[CreateFunction(typeof(Dependency), "CreateDep")]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        using var container = new Container();
        var instance = await container.CreateDepAsync().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}