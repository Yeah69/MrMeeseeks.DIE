using System;
using Xunit;

namespace MrMeeseeks.DIE.Test;

internal interface IValueTupleBase
{
    (int _0, int _1, int _2, int _3, int _4, 
        int _5, int _6, int _7, int _8, int _9, 
        int _10, int _11, int _12, int _13, int _14, 
        int _15, int _16, int _17, int _18, int _19,
        int _20, int _21, int _22, int _23, int _24, 
        int _25) Dependency { get; }
}

internal class ValueTupleBase : IValueTupleBase
{
    public ValueTupleBase(
        (int _0, int _1, int _2, int _3, int _4, 
            int _5, int _6, int _7, int _8, int _9, 
            int _10, int _11, int _12, int _13, int _14, 
            int _15, int _16, int _17, int _18, int _19, 
            int _20, int _21, int _22, int _23, int _24, 
            int _25) dependency) =>
        Dependency = dependency;

    public (int _0, int _1, int _2, int _3, int _4, 
        int _5, int _6, int _7, int _8, int _9, 
        int _10, int _11, int _12, int _13, int _14, 
        int _15, int _16, int _17, int _18, int _19, 
        int _20, int _21, int _22, int _23, int _24, 
        int _25) Dependency { get; }
}

internal partial class ValueTupleContainer : IContainer<IValueTupleBase>
{
    private int _i;

    private int DIE_Counter() => _i++;
}

public partial class ValueTupleTests
{
    [Fact]
    public void ResolveValueTuple()
    {
        using var container = new ValueTupleContainer();
        var valueTupleBase = ((IContainer<IValueTupleBase>) container).Resolve();
        Assert.Equal(25, valueTupleBase.Dependency._25);
    }
}

internal interface INonSyntaxValueTupleBase
{
    ValueTuple<int, int, int, int, int, int, int, 
            ValueTuple<int, int, int, int, int, int, int, 
                ValueTuple<int, int, int, int, int, int, int,
                    ValueTuple<int, int, int, int, int>>>>
        Dependency { get; }
}

internal class NonSyntaxValueTupleBase : INonSyntaxValueTupleBase
{
    public NonSyntaxValueTupleBase(
        ValueTuple<int, int, int, int, int, int, int, 
                ValueTuple<int, int, int, int, int, int, int, 
                    ValueTuple<int, int, int, int, int, int, int,
                        ValueTuple<int, int, int, int, int>>>>
            dependency) =>
        Dependency = dependency;

    public ValueTuple<int, int, int, int, int, int, int, 
            ValueTuple<int, int, int, int, int, int, int, 
                ValueTuple<int, int, int, int, int, int, int,
                    ValueTuple<int, int, int, int, int>>>>
        Dependency { get; }
}

internal partial class NonSyntaxValueTupleContainer : IContainer<INonSyntaxValueTupleBase>
{
    private int _i;

    private int DIE_Counter() => _i++;
}

public partial class ValueTupleTests
{
    [Fact]
    public void ResolveNonSyntaxValueTuple()
    {
        using var container = new NonSyntaxValueTupleContainer();
        var nonSyntaxValueTupleBase = ((IContainer<INonSyntaxValueTupleBase>) container).Resolve();
        Assert.Equal(25, nonSyntaxValueTupleBase.Dependency.Item26);
    }
}

internal interface INonSyntaxSingleItemValueTupleBase
{
    ValueTuple<int>
        Dependency { get; }
}

internal class NonSyntaxSingleItemValueTupleBase : INonSyntaxSingleItemValueTupleBase
{
    public NonSyntaxSingleItemValueTupleBase(
        ValueTuple<int>
            dependency) =>
        Dependency = dependency;

    public ValueTuple<int>
        Dependency { get; }
}

internal partial class NonSyntaxSingleItemValueTupleContainer : IContainer<INonSyntaxSingleItemValueTupleBase>
{
    private int _i;

    private int DIE_Counter() => _i++;
}

public partial class ValueTupleTests
{
    [Fact]
    public void ResolveNonSyntaxSingleItemValueTuple()
    {
        using var container = new NonSyntaxSingleItemValueTupleContainer();
        var NonSyntaxSingleItemValueTupleBase = ((IContainer<INonSyntaxSingleItemValueTupleBase>) container).Resolve();
        Assert.Equal(0, NonSyntaxSingleItemValueTupleBase.Dependency.Item1);
    }
}

internal interface INonSyntaxDoubleItemValueTupleBase
{
    ValueTuple<int, int>
        Dependency { get; }
}

internal class NonSyntaxDoubleItemValueTupleBase : INonSyntaxDoubleItemValueTupleBase
{
    public NonSyntaxDoubleItemValueTupleBase(
        ValueTuple<int, int>
            dependency) =>
        Dependency = dependency;

    public ValueTuple<int, int>
        Dependency { get; }
}

internal partial class NonSyntaxDoubleItemValueTupleContainer : IContainer<INonSyntaxDoubleItemValueTupleBase>
{
    private int _i;

    private int DIE_Counter() => _i++;
}

public partial class ValueTupleTests
{
    [Fact]
    public void ResolveNonSyntaxDoubleItemValueTuple()
    {
        using var container = new NonSyntaxDoubleItemValueTupleContainer();
        var NonSyntaxDoubleItemValueTupleBase = ((IContainer<INonSyntaxDoubleItemValueTupleBase>) container).Resolve();
        Assert.Equal(1, NonSyntaxDoubleItemValueTupleBase.Dependency.Item2);
    }
}