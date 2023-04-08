using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionProps.WithDependency;

internal class Dependency
{
    public int Number { get; init; }
}

internal class OtherDependency
{
    public int Number => 69;
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    private Container() {}
    
    [UserDefinedPropertiesInjection(typeof(Dependency))]
    private void DIE_Props_Dependency(OtherDependency otherDependency, out int Number) => Number = otherDependency.Number;
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