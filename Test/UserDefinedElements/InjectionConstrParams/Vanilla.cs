using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionConstrParams.Vanilla;

internal class Dependency
{
    public int Number { get; }

    internal Dependency(int number) => Number = number;
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    [UserDefinedConstructorParametersInjection(typeof(Dependency))]
    private void DIE_ConstrParams_Dependency(out int number) => number = 69;
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = container.Create();
        Assert.Equal(69, instance.Number);
    }
}