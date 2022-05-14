using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Struct.RecordOneExplicitConstructor;

internal class Inner {}

internal record struct Dependency(Inner Inner);

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
        Assert.NotNull(value.Inner);
    }
}