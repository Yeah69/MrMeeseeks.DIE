using System;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Test.ValueTuple.NonSyntaxVariantSingleItem;

internal class Wrapper
{
    public Wrapper(
        ValueTuple<int>
            dependency) =>
        Dependency = dependency;

    public ValueTuple<int>
        Dependency { get; }
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    private int _i;

    private int DIE_Counter() => _i++;
}