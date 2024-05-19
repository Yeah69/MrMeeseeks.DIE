using System;
using System.Threading.Tasks;
using Xunit;
// ReSharper disable once CheckNamespace
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.TypeParameterMapping.OneToMany;

internal sealed class Dependency<T0, T1, T2>
{
    internal required T0 Value0 { get; init; }
    internal required T1 Value1 { get; init; }
    internal required T2 Value2 { get; init; }
}

[CreateFunction(typeof(Dependency<,,>), "Create")]
internal sealed partial class Container<
    [GenericParameterMapping(typeof(Dependency<,,>), "T0")] 
    [GenericParameterMapping(typeof(Dependency<,,>), "T1")] 
    [GenericParameterMapping(typeof(Dependency<,,>), "T2")] TA>
{
    private TA DIE_Factory_Value0()
    {
        if (typeof(TA) == typeof(int))
            return (TA)(object)69;
        throw new InvalidOperationException("Better be an int!");
    }
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container<int>.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Dependency<int, int, int>>(instance);
        Assert.Equal(69, instance.Value0);
        Assert.Equal(69, instance.Value1);
        Assert.Equal(69, instance.Value2);
    }
}