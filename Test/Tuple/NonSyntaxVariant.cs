using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Tuple.NonSyntaxVariant;

internal class Wrapper
{
    public Wrapper(
        Tuple<int, int, int, int, int, int, int, 
                Tuple<int, int, int, int, int, int, int, 
                    Tuple<int, int, int, int, int, int, int,
                        Tuple<int, int, int, int, int>>>>
            dependency) =>
        Dependency = dependency;

    public Tuple<int, int, int, int, int, int, int, 
            Tuple<int, int, int, int, int, int, int, 
                Tuple<int, int, int, int, int, int, int,
                    Tuple<int, int, int, int, int>>>>
        Dependency { get; }
}

[CreateFunction(typeof(Wrapper), "Create")]
internal sealed partial class Container
{
    private int _i;

    private int DIE_Counter() => _i++;
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var nonSyntaxValueTupleBase = container.Create();
        Assert.Equal(25, nonSyntaxValueTupleBase.Dependency.Rest.Rest.Rest.Item5);
    }
}