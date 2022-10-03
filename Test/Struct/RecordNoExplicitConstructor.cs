using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Struct.RecordNoExplicitConstructor;

internal record struct Dependency;

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var value = container.Create();
        Assert.IsType<Dependency>(value);
    }
}