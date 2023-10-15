using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

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

[InjectionKey(Key.A)]
internal class DependencyA : IInterface
{
}

[InjectionKey(Key.B)]
internal class DependencyB : IInterface
{
}

[InjectionKey(Key.C)]
internal class DependencyC : IInterface
{
}

internal class Root
{
    [InjectionKey(Key.B)]
    internal required IInterface Dependency { get; init; }
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    private Container() {}
}