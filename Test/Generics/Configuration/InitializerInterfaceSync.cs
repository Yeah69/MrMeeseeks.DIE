using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.InitializerInterfaceSync;

internal class Dependency<T0> : ITypeInitializer
{
    void ITypeInitializer.Initialize()
    {
        IsInitialized = true;
    }

    public bool IsInitialized { get; private set; }
}

[CreateFunction(typeof(Dependency<int>), "Create")]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var instance = container.Create();
        Assert.True(instance.IsInitialized);
    }
}