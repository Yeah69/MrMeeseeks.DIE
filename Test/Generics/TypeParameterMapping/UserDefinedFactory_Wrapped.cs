using System;
using System.Threading.Tasks;
using Xunit;
// ReSharper disable once CheckNamespace
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.TypeParameterMapping.UserDefinedFactory_Wrapped;

internal sealed record Wrapper<T>(T Value);

internal sealed class Dependency<T0, T1, T2>
{
    internal required Wrapper<T0> Value0 { get; init; }
    internal required Wrapper<T1> Value1 { get; init; }
    internal required Wrapper<T2> Value2 { get; init; }
}

[CreateFunction(typeof(Dependency<,,>), "Create")]
internal sealed partial class Container<
    [GenericParameterMapping(typeof(Dependency<,,>), "T0")] TA, 
    [GenericParameterMapping(typeof(Dependency<,,>), "T1")] TB,
    [GenericParameterMapping(typeof(Dependency<,,>), "T2")] TC>
{
    private Wrapper<TA> DIE_Factory_Value0()
    {
        if (typeof(TA) == typeof(int))
            return new((TA)(object)69);
        throw new InvalidOperationException("Better be an int!");
    }
    private Wrapper<TB> DIE_Factory_Value1 = new((TB)(object)3L);
    private Wrapper<TC> DIE_Factory_Value2 => new((TC)(object)23.0);
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container<int, long, double>.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Dependency<int, long, double>>(instance);
        Assert.Equal(69, instance.Value0.Value);
        Assert.Equal(3L, instance.Value1.Value);
        Assert.Equal(23.0, instance.Value2.Value);
    }
}