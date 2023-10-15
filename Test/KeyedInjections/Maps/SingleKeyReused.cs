using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.Maps.SingleKeyReused;

internal enum Key
{
    A,
    B
}

internal interface IInterface
{
}

[InjectionKey(Key.A)]
internal class DependencyA0 : IInterface
{
}

[InjectionKey(Key.A)]
internal class DependencyA1 : IInterface
{
}

[InjectionKey(Key.B)]
internal class DependencyB : IInterface
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
        Assert.False(map.TryGetValue(Key.A, out _));
        Assert.True(map.TryGetValue(Key.B, out var b));
        Assert.IsType<DependencyB>(b);
    }
}