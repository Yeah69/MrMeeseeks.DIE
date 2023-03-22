using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Property.Vanilla;

internal class Dependency {}

internal class Wrapper
{
    public Dependency? Dependency { get; init; }
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var hasInitProperty = container.Create();
        Assert.NotNull(hasInitProperty);
        Assert.NotNull(hasInitProperty.Dependency);
        Assert.IsType<Dependency>(hasInitProperty.Dependency);
    }
}