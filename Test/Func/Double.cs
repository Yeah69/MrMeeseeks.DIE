using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Func.Double;

internal class Dependency{}

internal class Parent
{
    internal Parent(
        Func<Dependency> fac0,
        Func<Dependency> fac1)
    {
        
    }
}

[CreateFunction(typeof(Parent), "Create")]
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
        var parent = container.Create();
        Assert.IsType<Parent>(parent);
    }
}
