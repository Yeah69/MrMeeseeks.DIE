using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal interface II;
[InjectionKey(0)]
internal class IX : II;
[InjectionKey(1)]
internal class IY : II;

/*internal class A : IDecorator<II>, II
{
    internal required II I { get; init; }
}

internal class B : IDecorator<II>, II
{
    internal required II I { get; init; }
}

internal class C : IDecorator<II>, II
{
    internal required II I { get; init; }
}*/

internal class Parent
{
    internal required IEnumerable<KeyValuePair<int, II>> Is { get; init; }
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container;
