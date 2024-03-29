using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Record.CustomProperty;

internal record Dependency;
internal record DependencyA;

internal record Implementation(Dependency Dependency)
{
    internal DependencyA? DependencyA { get; init; }
}

[PropertyChoice(typeof(Implementation), nameof(Implementation.DependencyA))]
[CreateFunction(typeof(Implementation), "Create")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Implementation>(instance);
        Assert.IsType<Dependency>(instance.Dependency);
        Assert.IsType<DependencyA>(instance.DependencyA);
    }
}