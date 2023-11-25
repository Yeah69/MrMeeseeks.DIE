using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.ValueTuple.NonSyntaxVariant;

internal class Wrapper(ValueTuple<int, int, int, int, int, int, int, 
        ValueTuple<int, int, int, int, int, int, int, 
            ValueTuple<int, int, int, int, int, int, int,
                ValueTuple<int, int, int, int, int>>>>
    dependency)
{
    public ValueTuple<int, int, int, int, int, int, int, 
            ValueTuple<int, int, int, int, int, int, int, 
                ValueTuple<int, int, int, int, int, int, int,
                    ValueTuple<int, int, int, int, int>>>>
        Dependency { get; } = dependency;
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    private int _i;

    private int DIE_Factory_Counter() => _i++;
    
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var nonSyntaxValueTupleBase = container.Create();
        Assert.Equal(25, nonSyntaxValueTupleBase.Dependency.Item26);
    }
}