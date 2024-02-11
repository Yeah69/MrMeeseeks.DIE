using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.BaseClassing.Class.ReuseOnContainerAndScopeWithContainerSpecificsIgnored;

internal class ScopeRoot
{
    internal int Number { get; init; }
}

[ScopeRootImplementationAggregation(typeof(ScopeRoot))]
[CreateFunction(typeof(int), "Create")]
[CreateFunction(typeof(ScopeRoot), "CreateScopeRoot")]
internal abstract class RangeBase
{
    protected int DIE_Factory_Int => 69;
}

internal sealed partial class Container : RangeBase
{
    private sealed partial class DIE_DefaultScope : RangeBase { }
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var number = container.Create();
        Assert.Equal(69, number);
        var scopeRoot = container.CreateScopeRoot();
        Assert.Equal(69, scopeRoot.Number);
    }
}