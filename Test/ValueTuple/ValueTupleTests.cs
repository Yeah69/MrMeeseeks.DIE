using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.ValueTuple.NonSyntaxVariantDoubleItem;

internal class Wrapper
{
    public Wrapper(
        ValueTuple<int, int>
            dependency) =>
        Dependency = dependency;

    public ValueTuple<int, int>
        Dependency { get; }
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    private int _i;

    private int DIE_Factory_Counter() => _i++;
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var wrapper = container.Create();
        Assert.Equal(1, wrapper.Dependency.Item2);
    }
}