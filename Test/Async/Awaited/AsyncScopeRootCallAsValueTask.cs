using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Awaited.AsyncScopeRootCallAsValueTask;


internal class Dependency : ITaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class ScopeRoot : IScopeRoot
{
    public Dependency Dep { get; }
    internal ScopeRoot(Dependency dep)
    {
        Dep = dep;
    }
}

[CreateFunction(typeof(ValueTask<ScopeRoot>), "Create")]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        using var container = new Container();
        var root = await container.Create().ConfigureAwait(false);
        Assert.True(root.Dep.IsInitialized);
    }
}