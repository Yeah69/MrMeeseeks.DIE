using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.Maps.SingleSimple;

internal enum Key
{
    A,
    B,
    C
}

internal interface IInterface
{
}

[InjectionKey(Key.A)]
internal class DependencyA : IInterface
{
}

[InjectionKey(Key.B)]
internal class DependencyB : IInterface
{
}

[InjectionKey(Key.C)]
internal class DependencyC : IInterface
{
}

[CreateFunction(typeof(IReadOnlyDictionary<Key, IInterface>), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var map = container.Create();
        Assert.True(map.TryGetValue(Key.A, out var a));
        Assert.IsType<DependencyA>(a);
        Assert.True(map.TryGetValue(Key.B, out var b));
        Assert.IsType<DependencyB>(b);
        Assert.True(map.TryGetValue(Key.C, out var c));
        Assert.IsType<DependencyC>(c);
    }
}