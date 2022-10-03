using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Struct.RangedInScope;

internal struct Dependency : IScopeInstance
{
    public int Value { get; set; }

    internal Dependency(int value) => 
        Value = value;
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    private int DIE_Factory_int => 23;
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var value = container.Create();
        Assert.IsType<Dependency>(value);
        Assert.Equal(23, value.Value);
        value.Value = 69;
        var valueAgain = container.Create();
        Assert.Equal(23, valueAgain.Value);
    }
}