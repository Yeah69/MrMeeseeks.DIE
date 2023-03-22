using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionConstrParams.WithDependency;

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
    private Container() {}
    
    [UserDefinedConstructorParametersInjection(typeof(Dependency))]
    private void DIE_ConstrParams_Dependency(OtherDependency otherDependency, out int number) => number = otherDependency.Number;
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.Equal(69, instance.Number);
    }
}