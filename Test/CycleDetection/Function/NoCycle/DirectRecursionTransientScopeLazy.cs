using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.CycleDetection.Function.NoCycle.DirectRecursionTransientScopeLazy;

internal class Dependency : ITransientScopeInstance
{
    // ReSharper disable once UnusedParameter.Local
    internal Dependency(Lazy<Dependency> inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
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
        var instance = container.Create();
        Assert.IsType<Dependency>(instance);
    }
}