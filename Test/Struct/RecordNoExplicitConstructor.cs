using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Struct.RecordNoExplicitConstructor;

internal record struct Dependency;

[CreateFunction(typeof(Dependency), "Create")]
internal partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var value = container.Create();
        Assert.IsType<Dependency>(value);
    }
}