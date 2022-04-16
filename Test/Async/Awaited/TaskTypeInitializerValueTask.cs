using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Awaited.TaskTypeInitializerValueTask;


internal class Dependency : IValueTaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    async ValueTask IValueTaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

[CreateFunction(typeof(Dependency), "Create")]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        var container = new Container();
        var instance = await container.CreateAsync().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}