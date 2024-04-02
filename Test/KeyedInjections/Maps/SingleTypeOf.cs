using System;
using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.Maps.SingleTypeOf;

internal interface IInterface;

[InjectionKey(typeof(DependencyA))]
internal sealed class DependencyA : IInterface;

[InjectionKey(typeof(DependencyB))]
internal sealed class DependencyB : IInterface;

[InjectionKey(typeof(DependencyC))]
internal sealed class DependencyC : IInterface;

[CreateFunction(typeof(IReadOnlyDictionary<Type, IInterface>), "Create")]
internal sealed partial class Container;

public sealed class Tests
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