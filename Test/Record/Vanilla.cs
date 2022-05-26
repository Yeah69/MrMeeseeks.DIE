using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Record.Vanilla;

internal record Dependency;

[CreateFunction(typeof(Dependency), "Create")]
internal partial class Container{}

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