using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;
using MrMeeseeks.DIE.UserUtility;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.KeyedInjectionsMultiple;

internal enum KeyValues { A, B, C }

internal interface IInterface<T>;

[InjectionKey(KeyValues.A)]
internal sealed class DependencyA0<T> : IInterface<T>;
[InjectionKey(KeyValues.A)]
internal sealed class DependencyA1<T> : IInterface<T>;
internal sealed class DependencyB0<T> : IInterface<T>;
internal sealed class DependencyB1<T> : IInterface<T>;
[InjectionKey(KeyValues.C)]
internal sealed class DependencyC0<T> : IInterface<T>;
[InjectionKey(KeyValues.C)]
internal sealed class DependencyC1<T> : IInterface<T>;

internal sealed class Root<TA, TB>
{
    [InjectionKey(KeyValues.A)]
    internal required IReadOnlyList<IInterface<TA>> DependencyA { get; init; }
    [InjectionKey(KeyValues.B)]
    internal required IReadOnlyList<IInterface<TB>> DependencyB { get; init; }
    internal required IReadOnlyDictionary<KeyValues, IReadOnlyList<IInterface<TA>>> AllDependenciesA { get; init; }
    internal required IReadOnlyDictionary<KeyValues, IReadOnlyList<IInterface<TB>>> AllDependenciesB { get; init; }
}

[InjectionKeyChoice(KeyValues.B, typeof(DependencyB0<>))]
[InjectionKeyChoice(KeyValues.B, typeof(DependencyB1<>))]
[CreateFunction(typeof(Root<,>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var root = container.Create<int, string>();
        
        foreach (var dependencyA in root.DependencyA)
            Assert.True(dependencyA is DependencyA0<int> || dependencyA is DependencyA1<int>);
        foreach (var dependencyB in root.DependencyB)
            Assert.True(dependencyB is DependencyB0<string> || dependencyB is DependencyB1<string>);
        foreach (var dependencyA in root.AllDependenciesA[KeyValues.A])
            Assert.True(dependencyA is DependencyA0<int> || dependencyA is DependencyA1<int>);
        foreach (var dependencyB in root.AllDependenciesA[KeyValues.B])
            Assert.True(dependencyB is DependencyB0<int> || dependencyB is DependencyB1<int>);
        foreach (var dependencyC in root.AllDependenciesA[KeyValues.C])
            Assert.True(dependencyC is DependencyC0<int> || dependencyC is DependencyC1<int>);
        foreach (var dependencyA in root.AllDependenciesB[KeyValues.A])
            Assert.True(dependencyA is DependencyA0<string> || dependencyA is DependencyA1<string>);
        foreach (var dependencyB in root.AllDependenciesB[KeyValues.B])
            Assert.True(dependencyB is DependencyB0<string> || dependencyB is DependencyB1<string>);
        foreach (var dependencyC in root.AllDependenciesB[KeyValues.C])
            Assert.True(dependencyC is DependencyC0<string> || dependencyC is DependencyC1<string>);
    }
}