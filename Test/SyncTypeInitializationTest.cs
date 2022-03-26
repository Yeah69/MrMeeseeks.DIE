using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.SyncTypeInitializationTest;

internal class Dependency : ITypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    void ITypeInitializer.Initialize() => IsInitialized = true;
}

[CreateFunction(typeof(Dependency), "Create0")]
internal partial class Container 
{
}

public partial class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = container.Create0();
        Assert.True(instance.IsInitialized);
    }
}