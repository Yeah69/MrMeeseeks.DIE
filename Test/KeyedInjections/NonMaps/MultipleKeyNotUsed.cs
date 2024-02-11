using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.NonMaps.MultipleKeyNotUsed;

internal enum Key
{
    A,
    B
}

internal interface IInterface { }

[InjectionKey(Key.B)]
internal class DependencyB : IInterface { }

internal class Root
{
    public Root([InjectionKey(Key.A)] IReadOnlyList<IInterface> dependencies) => Dependencies = dependencies;

    public IReadOnlyList<IInterface> Dependencies { get; }
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var root = container.Create();
        Assert.Empty(root.Dependencies);
    }
}