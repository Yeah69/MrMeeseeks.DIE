using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionConstrParams.WithAsyncDependencyInScope;

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

internal sealed class ScopeRoot : IScopeRoot
{
    public Dependency Dependency { get; }

    internal ScopeRoot(
        Dependency dependency) => Dependency = dependency;
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container
{
    
    
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultScope
    {
        [UserDefinedConstructorParametersInjection(typeof(Dependency))]
        private void DIE_ConstrParams_Dependency(OtherDependency otherDependency, out int number) => number = otherDependency.Number;
    }
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = await container.Create();
        Assert.Equal(69, instance.Dependency.Number);
    }
}