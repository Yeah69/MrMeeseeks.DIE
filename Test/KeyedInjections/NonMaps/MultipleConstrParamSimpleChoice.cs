using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.NonMaps.MultipleConstrParamSimpleChoice;

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

internal class Root
{
    public Root([InjectionKey(Key.B)] IReadOnlyList<IInterface> dependencies) => Dependencies = dependencies;

    public IReadOnlyList<IInterface> Dependencies { get; }
}

[InjectionKeyChoice(Key.A, typeof(DependencyA0))]
[InjectionKeyChoice(Key.A, typeof(DependencyA1))]
[InjectionKeyChoice(Key.B, typeof(DependencyB0))]
[InjectionKeyChoice(Key.B, typeof(DependencyB1))]
[InjectionKeyChoice(Key.C, typeof(DependencyC0))]
[InjectionKeyChoice(Key.C, typeof(DependencyC1))]
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