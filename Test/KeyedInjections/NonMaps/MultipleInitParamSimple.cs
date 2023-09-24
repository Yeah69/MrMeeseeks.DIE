using System.Collections.Generic;
using System.Linq;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.NonMaps.MultipleInitParamSimple;

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
    internal IReadOnlyList<IInterface>? Dependencies { get; private set; }

    public void Initialize([Key(Key.B)] IReadOnlyList<IInterface> dependencies) => Dependencies = dependencies;
}

[Initializer(typeof(Root), nameof(Root.Initialize))]
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
        Assert.NotNull(root.Dependencies);
        foreach (var dependency in root.Dependencies ?? Enumerable.Empty<IInterface>())
            Assert.True(dependency is DependencyB0 or DependencyB1);
    }
}