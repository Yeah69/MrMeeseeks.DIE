using System;
using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration;
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
