using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Record.Vanilla;

internal record Dependency;

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container;

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