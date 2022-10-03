using System;
using MrMeeseeks.DIE.Configuration.Attributes;
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
        Assert.Equal(0, wrapper.Dependency.Item1);
    }
}