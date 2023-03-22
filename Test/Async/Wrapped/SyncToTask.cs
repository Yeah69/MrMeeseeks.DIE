using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Wrapped.SyncToTask;

internal class Dependency : IInitializer
{
    public bool IsInitialized { get; private set; }
    
    void IInitializer.Initialize()
    {
        IsInitialized = true;
    }
}

[CreateFunction(typeof(Task<Dependency>), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = await container.Create().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}