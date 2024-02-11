using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionProps.Vanilla;

internal class Dependency
{
    public int Number { get; init; }
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    
    
    [UserDefinedPropertiesInjection(typeof(Dependency))]
    // ReSharper disable once InconsistentNaming
    private void DIE_Props_Dependency(out int Number) => Number = 69;
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