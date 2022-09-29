using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CycleDetection.Implementation.InParallel;

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
internal sealed partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var parent = container.Create();
        Assert.IsType<Parent>(parent);
    }
}