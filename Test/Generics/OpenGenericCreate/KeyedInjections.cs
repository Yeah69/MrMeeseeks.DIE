using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;
using MrMeeseeks.DIE.UserUtility;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.KeyedInjections;

internal enum KeyValues { A, B, C }

internal interface IInterface<T>;

[InjectionKey(KeyValues.A)]
internal class DependencyA<T> : IInterface<T>;
internal class DependencyB<T> : IInterface<T>;
[InjectionKey(KeyValues.C)]
internal class DependencyC<T> : IInterface<T>;

internal class Root<TA, TB>
{
    [InjectionKey(KeyValues.A)]
    internal required IInterface<TA> DependencyA { get; init; }
    [InjectionKey(KeyValues.B)]
    internal required IInterface<TB> DependencyB { get; init; }
    internal required IReadOnlyDictionary<KeyValues, IInterface<TA>> AllDependenciesA { get; init; }
    internal required IReadOnlyDictionary<KeyValues, IInterface<TB>> AllDependenciesB { get; init; }
}

[InjectionKeyChoice(KeyValues.B, typeof(DependencyB<>))]
[CreateFunction(typeof(Root<,>), "Create")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var root = container.Create<int, string>();
        
        Assert.IsType<DependencyA<int>>(root.DependencyA);
        Assert.IsType<DependencyB<string>>(root.DependencyB);
        Assert.IsType<DependencyA<int>>(root.AllDependenciesA[KeyValues.A]);
        Assert.IsType<DependencyB<int>>(root.AllDependenciesA[KeyValues.B]);
        Assert.IsType<DependencyC<int>>(root.AllDependenciesA[KeyValues.C]);
        Assert.IsType<DependencyA<string>>(root.AllDependenciesB[KeyValues.A]);
        Assert.IsType<DependencyB<string>>(root.AllDependenciesB[KeyValues.B]);
        Assert.IsType<DependencyC<string>>(root.AllDependenciesB[KeyValues.C]);
    }
}