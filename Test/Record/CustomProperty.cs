using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Record.CustomPropert;

internal record Dependency;
internal record DependencyA;

internal record Implementation(Dependency Dependency)
{
    internal DependencyA? DependencyA { get; init; }
}

[PropertyChoice(typeof(Implementation), nameof(Implementation.DependencyA))]
[CreateFunction(typeof(Implementation), "Create")]
internal partial class Container{}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var instance = container.Create();
        Assert.IsType<Implementation>(instance);
        Assert.IsType<Dependency>(instance.Dependency);
        Assert.IsType<DependencyA>(instance.DependencyA);
    }
}