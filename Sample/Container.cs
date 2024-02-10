using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal enum KeyValues { A, B, C }

internal interface IInterface<T> { }

[InjectionKey(KeyValues.A)]
internal class DependencyA<T> : IInterface<T> { }
internal class DependencyB<T> : IInterface<T> { }
[InjectionKey(KeyValues.C)]
internal class DependencyC<T> : IInterface<T> { }

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
internal sealed partial class Container
{
    private Container() {}
}