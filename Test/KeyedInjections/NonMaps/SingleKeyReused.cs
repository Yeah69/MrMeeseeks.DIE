﻿using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.NonMaps.SingleKeyReused;

internal enum Key
{
    A,
    B
}

internal interface IInterface
{
}

[Key(Key.A)]
internal class DependencyA0 : IInterface
{
}

[Key(Key.A)]
internal class DependencyA1 : IInterface
{
}

[Key(Key.B)]
internal class DependencyB : IInterface
{
}

internal class Root
{
    public Root([Key(Key.A)] IInterface? dependency) => Dependency = dependency;

    public IInterface? Dependency { get; }
}

[CreateFunction(typeof(Root), "Create")]
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
        var root = container.Create();
        Assert.Null(root.Dependency);
    }
}