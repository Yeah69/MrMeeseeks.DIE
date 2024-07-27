using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.Test.Wrappers.ValueTuple.NonSyntaxVariant;

internal sealed class Wrapper
{
    public Wrapper(
        ValueTuple<int, int, int, int, int, int, int, 
                ValueTuple<int, int, int, int, int, int, int, 
                    ValueTuple<int, int, int, int, int, int, int,
                        ValueTuple<int, int, int, int, int>>>>
            dependency) =>
        Dependency = dependency;

    public ValueTuple<int, int, int, int, int, int, int, 
            ValueTuple<int, int, int, int, int, int, int, 
                ValueTuple<int, int, int, int, int, int, int,
                    ValueTuple<int, int, int, int, int>>>>
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
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var nonSyntaxValueTupleBase = container.Create();
        Assert.Equal(25, nonSyntaxValueTupleBase.Dependency.Item26);
    }
}