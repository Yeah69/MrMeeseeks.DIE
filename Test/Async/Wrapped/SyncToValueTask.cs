using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Async.Wrapped.SyncToValueTask;

internal sealed class Dependency : IInitializer
{
    public bool IsInitialized { get; private set; }
    
    void IInitializer.Initialize()
    {
        IsInitialized = true;
    }
}

[CreateFunction(typeof(ValueTask<Dependency>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = await container.Create();
        Assert.True(instance.IsInitialized);
    }
}