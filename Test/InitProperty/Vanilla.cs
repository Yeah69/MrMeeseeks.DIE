using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.InitProperty.Vanilla;

internal class Dependency {}

internal class Wrapper
{
    public Dependency? Dependency { get; init; }
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var hasInitProperty = container.Create();
        Assert.NotNull(hasInitProperty);
        Assert.NotNull(hasInitProperty.Dependency);
        Assert.IsType<Dependency>(hasInitProperty.Dependency);
    }
}