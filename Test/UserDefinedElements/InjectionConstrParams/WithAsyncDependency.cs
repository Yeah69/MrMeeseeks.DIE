using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionConstrParams.WithAsyncDependency;

internal sealed class Dependency
{
    public int Number { get; }

    internal Dependency(int number) => Number = number;
}

internal sealed class OtherDependency : IValueTaskInitializer
{
    public int Number => 69;
    public ValueTask InitializeAsync() => new (Task.CompletedTask);
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
        var instance = await container.Create();
        Assert.Equal(69, instance.Number);
    }
}