using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.Maps.SimpleMultipleChoice;

internal enum Key
{
    A,
    B,
    C
}

internal interface IInterface
{
}

internal class DependencyA0 : IInterface
{
}

internal class DependencyA1 : IInterface
{
}

internal class DependencyB0 : IInterface
{
}

internal class DependencyB1 : IInterface
{
}

internal class DependencyC0 : IInterface
{
}

internal class DependencyC1 : IInterface
{
}

[InjectionKeyChoice(Key.A, typeof(DependencyA0))]
[InjectionKeyChoice(Key.A, typeof(DependencyA1))]
[InjectionKeyChoice(Key.B, typeof(DependencyB0))]
[InjectionKeyChoice(Key.B, typeof(DependencyB1))]
[InjectionKeyChoice(Key.C, typeof(DependencyC0))]
[InjectionKeyChoice(Key.C, typeof(DependencyC1))]
[CreateFunction(typeof(IReadOnlyDictionary<Key, IReadOnlyList<IInterface>>), "Create")]
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