using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.CycleDetection.NoCycleInParallel;

internal class Dependency
{
}

internal class Parent
{
    internal Parent(
        Dependency dep0,
        Dependency dep1)
    {
        
    }
}

[CreateFunction(typeof(Parent), "Create")]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var parent = container.Create();
        Assert.IsType<Parent>(parent);
    }
}