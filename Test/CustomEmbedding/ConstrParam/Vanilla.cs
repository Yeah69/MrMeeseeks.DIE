using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.ConstrParam.Vanilla;

internal class Dependency
{
    public int Number { get; }

    internal Dependency(int number) => Number = number;
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    [UserDefinedConstrParamsInjection(typeof(Dependency))]
    private void DIE_ConstrParam_Dependency(out int number) => number = 69;
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.Equal(69, instance.Number);
    }
}