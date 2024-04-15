using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.BaseClassing.Class.ReuseOnContainerAndScope;

internal sealed class ScopeRoot
{
    internal int Number { get; init; }
}

internal abstract class RangeBase
{
    protected int DIE_Factory_Int => 69;
}

[CreateFunction(typeof(int), "Create")]
[CreateFunction(typeof(ScopeRoot), "CreateScopeRoot")]
internal sealed partial class Container : RangeBase
{
    private sealed partial class DIE_DefaultScope : RangeBase;
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var number = container.Create();
        Assert.Equal(69, number);
        var scopeRoot = container.CreateScopeRoot();
        Assert.Equal(69, scopeRoot.Number);
    }
}