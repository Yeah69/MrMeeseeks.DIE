using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Initializer.Sync;

internal class Dependency : IInitializer
{
    public bool IsInitialized { get; private set; }
    
    void IInitializer.Initialize() => IsInitialized = true;
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container 
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.True(instance.IsInitialized);
    }
}