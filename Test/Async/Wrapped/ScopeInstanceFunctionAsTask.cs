using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Wrapped.ScopeInstanceFunctionAsTask;


internal class Dependency : ITaskTypeInitializer, IScopeInstance
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

[CreateFunction(typeof(Task<Dependency>), "Create")]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        var container = new Container();
        var instance = container.Create();
        Assert.True((await instance.ConfigureAwait(false)).IsInitialized);
    }
}