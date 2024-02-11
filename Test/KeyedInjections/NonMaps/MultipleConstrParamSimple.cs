﻿using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.NonMaps.MultipleConstrParamSimple;

internal enum Key
{
    A,
    B,
    C
}

internal interface IInterface { }

[InjectionKey(Key.A)]
internal class DependencyA0 : IInterface { }

[InjectionKey(Key.A)]
internal class DependencyA1 : IInterface { }

[InjectionKey(Key.B)]
internal class DependencyB0 : IInterface { }

[InjectionKey(Key.B)]
internal class DependencyB1 : IInterface { }

[InjectionKey(Key.C)]
internal class DependencyC0 : IInterface { }

[InjectionKey(Key.C)]
internal class DependencyC1 : IInterface { }

internal class Root
{
    public Root([InjectionKey(Key.B)] IReadOnlyList<IInterface> dependencies) => Dependencies = dependencies;

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
        foreach (var dependency in root.Dependencies)
            Assert.True(dependency is DependencyB0 or DependencyB1);
    }
}