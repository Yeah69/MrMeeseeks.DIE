using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Awaited.ScopeInstanceFunctionAsValueTask;


internal class Dependency : ITaskTypeInitializer, IScopeInstance
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
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
        using var container = new Container();
        var instance = container.CreateValueAsync();
        Assert.True((await instance.ConfigureAwait(false)).IsInitialized);
    }
}