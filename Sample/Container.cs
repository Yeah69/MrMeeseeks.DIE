using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal interface II;
//[InjectionKey(0)]
internal class IX : II;
/*[InjectionKey(1)]
internal class IY : II;

internal class A : IDecorator<II>, II
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

internal class a(II i) : IScopeRoot
{
    internal II I => i;
}

internal class b(II i) : IScopeRoot
{
    internal II I => i;
}

internal class Parent(a aa, b bb)
{
    internal required a a { get; init; }
    internal required b b { get; init; }
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container
{
    //[DecoratorSequenceChoice(typeof(II), typeof(II), typeof(A), typeof(B), typeof(C))]
    [CustomScopeForRootTypes(typeof(a))]
    private sealed partial class DIE_Scope_a;
    //[DecoratorSequenceChoice(typeof(II), typeof(II), typeof(A), typeof(C))]
    [CustomScopeForRootTypes(typeof(b))]
    private sealed partial class DIE_Scope_b;
}
