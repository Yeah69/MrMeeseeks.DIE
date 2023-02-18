using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Awaited.AsyncScopeRootCallAwaited;


internal class Dependency : ITaskInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskInitializer.InitializeAsync()
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

[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container
{
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = new Container();
        var root = await container.Create().ConfigureAwait(false);
        Assert.True(root.Dep.IsInitialized);
    }
}