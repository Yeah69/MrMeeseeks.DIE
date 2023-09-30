﻿using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

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
internal abstract partial class ContainerBase
{
}

[InjectionKeyChoice(Key.B, typeof(DependencyB1))]
[InjectionKeyChoice(Key.C, typeof(DependencyC0))]
[InjectionKeyChoice(Key.C, typeof(DependencyC1))]
[CreateFunction(typeof(IReadOnlyDictionary<Key, IReadOnlyList<IInterface>>), "Create")]
internal partial class Container : ContainerBase
{
    private Container() {}
}