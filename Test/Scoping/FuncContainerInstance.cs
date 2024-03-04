using MrMeeseeks.DIE.Configuration.Attributes;
using System;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.FuncContainerInstance;

internal class Dependency : IContainerInstance;

[CreateFunction(typeof(Func<string, Dependency>), "Create")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create()("Foo");
        Assert.IsType<Dependency>(instance);
    }
}