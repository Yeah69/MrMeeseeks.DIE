using System;
using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.Maps.SingleTypeOf;

internal interface IInterface
{
}

[Key(typeof(DependencyA))]
internal class DependencyA : IInterface
{
}

[Key(typeof(DependencyB))]
internal class DependencyB : IInterface
{
}

[Key(typeof(DependencyC))]
internal class DependencyC : IInterface
{
}

[CreateFunction(typeof(IReadOnlyDictionary<Type, IInterface>), "Create")]
internal partial class Container
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
        Assert.True(map.TryGetValue(typeof(DependencyA), out var a));
        Assert.IsType<DependencyA>(a);
        Assert.True(map.TryGetValue(typeof(DependencyB), out var b));
        Assert.IsType<DependencyB>(b);
        Assert.True(map.TryGetValue(typeof(DependencyC), out var c));
        Assert.IsType<DependencyC>(c);
    }
}