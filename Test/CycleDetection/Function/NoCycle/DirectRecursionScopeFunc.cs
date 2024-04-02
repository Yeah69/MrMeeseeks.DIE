using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.CycleDetection.Function.NoCycle.DirectRecursionScopeFunc;

internal sealed class Dependency : IScopeInstance
{
    // ReSharper disable once UnusedParameter.Local
    internal Dependency(Func<Dependency> inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Dependency>(instance);
    }
}