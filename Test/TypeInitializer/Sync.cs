using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.TypeInitializer.Sync;

internal class Dependency : ITypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    void ITypeInitializer.Initialize() => IsInitialized = true;
}

[CreateFunction(typeof(Dependency), "Create")]
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