using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.Maps.SingleSimpleChoice;

internal enum Key
{
    A,
    B,
    C
}

internal interface IInterface;

internal sealed class DependencyA : IInterface;

internal sealed class DependencyB : IInterface;

internal sealed class DependencyC : IInterface;

[InjectionKeyChoice(Key.A, typeof(DependencyA))]
[InjectionKeyChoice(Key.B, typeof(DependencyB))]
[InjectionKeyChoice(Key.C, typeof(DependencyC))]
[CreateFunction(typeof(IReadOnlyDictionary<Key, IInterface>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var map = container.Create();
        Assert.True(map.TryGetValue(Key.A, out var a));
        Assert.IsType<DependencyA>(a);
        Assert.True(map.TryGetValue(Key.B, out var b));
        Assert.IsType<DependencyB>(b);
        Assert.True(map.TryGetValue(Key.C, out var c));
        Assert.IsType<DependencyC>(c);
    }
}