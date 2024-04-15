using MrMeeseeks.DIE.Configuration.Attributes;
using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.FuncContainerInstance;

internal sealed class Dependency : IContainerInstance;

[CreateFunction(typeof(Func<string, Dependency>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create()("Foo");
        Assert.IsType<Dependency>(instance);
    }
}