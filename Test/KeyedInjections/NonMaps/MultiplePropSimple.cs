using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.NonMaps.MultiplePropSimple;

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

internal class Root
{
    [Key(Key.B)]
    internal required IReadOnlyList<IInterface> Dependencies { get; init; }
}

[CreateFunction(typeof(Root), "Create")]
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
        var root = container.Create();
        foreach (var dependency in root.Dependencies)
            Assert.True(dependency is DependencyB0 or DependencyB1);
    }
}