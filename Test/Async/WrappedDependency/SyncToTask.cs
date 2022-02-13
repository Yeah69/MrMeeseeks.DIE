using System.Threading.Tasks;
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

internal partial class Container : IContainer<Task<Dependency>>
{
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = new Container();
        var instance = await ((IContainer<Task<Dependency>>) container).Resolve().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}