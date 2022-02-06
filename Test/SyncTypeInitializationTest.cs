using Xunit;

namespace MrMeeseeks.DIE.Test.SyncTypeInitializationTest;

internal class Dependency : ITypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    void ITypeInitializer.Initialize() => IsInitialized = true;
}

internal partial class Container 
    : IContainer<Dependency>
{
}

public partial class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = ((IContainer<Dependency>) container).Resolve();
        Assert.True(instance.IsInitialized);
    }
}