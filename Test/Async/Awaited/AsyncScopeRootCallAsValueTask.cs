using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Async.Awaited.AsyncScopeRootCallAsValueTask;


internal class Dependency : ITaskInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskInitializer.InitializeAsync()
    {
        await Task.Delay(500);
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
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var root = await container.Create();
        Assert.True(root.Dep.IsInitialized);
    }
}