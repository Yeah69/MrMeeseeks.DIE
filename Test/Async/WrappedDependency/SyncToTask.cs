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

[CreateFunction(typeof(Task<Dependency>), "CreateDep")]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = new Container();
        var instance = await container.CreateDep().ConfigureAwait(false);
        Assert.True(instance.IsInitialized);
    }
}