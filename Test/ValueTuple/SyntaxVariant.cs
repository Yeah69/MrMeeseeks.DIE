using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.ValueTuple.SyntaxVariant;

internal class Wrapper
{
    public Wrapper(
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

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    private int _i;

    private int DIE_Factory_Counter() => _i++;
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var valueTupleBase = container.Create();
        Assert.Equal(25, valueTupleBase.Dependency._25);
    }
}