using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CycleDetection.Function.NoCycle.DirectRecursionContainerLazy;

internal class Dependency : IContainerInstance
{
    internal Dependency(Lazy<Dependency> inner) {}
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
        Assert.IsType<Dependency>(instance);
    }
}