using System;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.ValueTuple.NonSyntaxVariantSingleItem;

internal class Wrapper
{
    public Wrapper(
        ValueTuple<int>
            dependency) =>
        Dependency = dependency;

    public ValueTuple<int>
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
        Assert.Equal(0, wrapper.Dependency.Item1);
    }
}