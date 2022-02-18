using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.WrappedDependency.SyncToTask;

internal class Dependency : ITypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    void ITypeInitializer.Initialize()
    {
        IsInitialized = true;
    }
}

[MultiContainer(typeof(Task<Dependency>))]
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