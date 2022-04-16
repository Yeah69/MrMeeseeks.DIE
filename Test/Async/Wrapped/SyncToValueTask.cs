using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Wrapped.SyncToValueTask;

internal class Dependency : ITypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    void ITypeInitializer.Initialize()
    {
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
        var container = new Container();
        var instance = await container.Create().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}