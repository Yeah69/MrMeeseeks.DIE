using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal class I
{
    internal I(int  value) =>
        Value = value;
    internal I(long valueLong) =>
        ValueLong = valueLong;
    internal int Value { get; }
    internal long ValueLong { get; }
}

internal class a(Func<int, I> iFactory) : IScopeRoot
{
    internal I I => iFactory(69);
}

internal class b(Func<long, I> iFactory) : IScopeRoot
{
    internal I I => iFactory(161L);
}

internal class Parent
{
    internal required a a { get; init; }
    internal required b b { get; init; }
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container
{
    [ConstructorChoice(typeof(I), typeof(int))]
    [CustomScopeForRootTypes(typeof(a))]
    sealed partial class DIE_Scope_a;
    [ConstructorChoice(typeof(I), typeof(long))]
    [CustomScopeForRootTypes(typeof(b))]
    sealed partial class DIE_Scope_b;
}
