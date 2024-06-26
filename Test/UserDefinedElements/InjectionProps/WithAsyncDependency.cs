using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionProps.WithAsyncDependency;

internal sealed class Dependency
{
    public int Number { get; init; }
}

internal sealed class OtherDependency : IValueTaskInitializer
{
    public int Number => 69;
    public ValueTask InitializeAsync() => new (Task.CompletedTask);
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    
    
    [UserDefinedPropertiesInjection(typeof(Dependency))]
    // ReSharper disable once InconsistentNaming
    private void DIE_Props_Dependency(OtherDependency otherDependency, out int Number) => Number = otherDependency.Number;
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = await container.Create();
        Assert.Equal(69, instance.Number);
    }
}