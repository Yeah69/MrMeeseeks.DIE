using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Tuple.NonSyntaxVariantDoubleItem;

internal class Wrapper
{
    public Wrapper(
        Tuple<int, int>
            dependency) =>
        Dependency = dependency;

    public Tuple<int, int>
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
        var wrapper = container.Create();
        Assert.Equal(1, wrapper.Dependency.Item2);
    }
}