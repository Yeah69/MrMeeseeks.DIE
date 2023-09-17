using System.Collections.Generic;
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