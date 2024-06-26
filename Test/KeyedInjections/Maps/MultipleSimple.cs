﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.Maps.SimpleMultiple;

internal enum Key
{
    A,
    B,
    C
}

internal interface IInterface;

[InjectionKey(Key.A)]
internal sealed class DependencyA0 : IInterface;

[InjectionKey(Key.A)]
internal sealed class DependencyA1 : IInterface;

[InjectionKey(Key.B)]
internal sealed class DependencyB0 : IInterface;

[InjectionKey(Key.B)]
internal sealed class DependencyB1 : IInterface;

[InjectionKey(Key.C)]
internal sealed class DependencyC0 : IInterface;

[InjectionKey(Key.C)]
internal sealed class DependencyC1 : IInterface;

[CreateFunction(typeof(IReadOnlyDictionary<Key, IReadOnlyList<IInterface>>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var map = container.Create();
        Assert.Equal(3, map.Count);
        Assert.Equal(2, map[Key.A].Count);
        foreach (var a in map[Key.A])
        {
            Assert.True(a is DependencyA0 or DependencyA1);
        }
        Assert.Equal(2, map[Key.B].Count);
        foreach (var b in map[Key.B])
        {
            Assert.True(b is DependencyB0 or DependencyB1);
        }
        Assert.Equal(2, map[Key.C].Count);
        foreach (var c in map[Key.C])
        {
            Assert.True(c is DependencyC0 or DependencyC1);
        }
    }
}