using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.ConstructorParameterWithDependency;

internal class Dependency
{
    public int Number { get; }

    internal Dependency(int number) => Number = number;
}

internal class OtherDependency
{
    public int Number => 69;
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    [CustomConstructorParameterChoice(typeof(Dependency))]
    private void DIE_ConstrParam_Dependency(OtherDependency otherDependency, out int number) => number = otherDependency.Number;
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