/*using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.Maps.SimpleMultiple;

internal enum Key
{
    A,
    B,
    C
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
internal class DependencyB0 : IInterface
{
}

[Key(Key.B)]
internal class DependencyB1 : IInterface
{
}

[Key(Key.C)]
internal class DependencyC0 : IInterface
{
}

[Key(Key.C)]
internal class DependencyC1 : IInterface
{
}

[CreateFunction(typeof(IReadOnlyDictionary<Key, IReadOnlyList<IInterface>>), "Create")]
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
        foreach (var a in map[Key.A])
        {
            Assert.True(a is DependencyA0 or DependencyA1);
        }
        foreach (var b in map[Key.B])
        {
            Assert.True(b is DependencyB0 or DependencyB1);
        }
        foreach (var c in map[Key.C])
        {
            Assert.True(c is DependencyC0 or DependencyC1);
        }
    }
}*/