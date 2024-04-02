using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Property.Vanilla;

internal sealed class Dependency;

internal sealed class Wrapper
{
    public Dependency? Dependency { get; init; }
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container;

public sealed class Tests
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