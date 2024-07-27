using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionConstrParams.WithDependency;

internal sealed class Dependency
{
    public int Number { get; }

    internal Dependency(int number) => Number = number;
}

internal sealed class OtherDependency
{
    public int Number => 69;
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    
    
    [UserDefinedConstructorParametersInjection(typeof(Dependency))]
    private void DIE_ConstrParams_Dependency(OtherDependency otherDependency, out int number) => number = otherDependency.Number;
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.Equal(69, instance.Number);
    }
}