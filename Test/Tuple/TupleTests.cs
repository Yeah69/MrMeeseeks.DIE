using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Tuple.NonSyntaxVariantDoubleItem;

internal sealed class Wrapper
{
    public Wrapper(
        Tuple<int, int>
            dependency) =>
        Dependency = dependency;

    public Tuple<int, int>
        Dependency { get; }
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    private int _i;

    private int DIE_Factory_Counter() => _i++;
    
}

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var wrapper = container.Create();
        Assert.Equal(1, wrapper.Dependency.Item2);
    }
}