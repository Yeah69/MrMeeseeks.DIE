using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.InitProperty.Vanilla;

internal class Dependency {}

internal class Wrapper
{
    public Dependency? Dependency { get; init; }
}

[CreateFunction(typeof(Wrapper), "Create")]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var hasInitProperty = container.Create();
        Assert.NotNull(hasInitProperty);
        Assert.NotNull(hasInitProperty.Dependency);
        Assert.IsType<Dependency>(hasInitProperty.Dependency);
    }
}