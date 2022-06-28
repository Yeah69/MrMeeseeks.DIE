using MrMeeseeks.DIE.Configuration.Attributes;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MrMeeseeks.DIE.Test.Scoping.FuncContainerInstance;

internal class Dependency : IContainerInstance {}

[CreateFunction(typeof(Func<string, Dependency>), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create()("Foo");
        Assert.IsType<Dependency>(instance);
    }
}