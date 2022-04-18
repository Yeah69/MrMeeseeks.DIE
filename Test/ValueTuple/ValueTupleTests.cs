using System;
using MrMeeseeks.DIE.Configuration;
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
internal partial class Container
{
    private int _i;

    private int DIE_Counter() => _i++;
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var wrapper = container.Create();
        Assert.Equal(1, wrapper.Dependency.Item2);
    }
}