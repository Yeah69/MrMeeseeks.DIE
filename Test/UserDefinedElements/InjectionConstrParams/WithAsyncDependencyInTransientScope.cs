using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionConstrParams.WithAsyncDependencyInTransientScope;

internal class Dependency
{
    public int Number { get; }

    internal Dependency(int number) => Number = number;
}

internal class OtherDependency : IValueTaskInitializer
{
    public int Number => 69;
    public ValueTask InitializeAsync() => new (Task.CompletedTask);
}

internal class TransientScopeRoot : ITransientScopeRoot
{
    public Dependency Dependency { get; }

    internal TransientScopeRoot(
        Dependency dependency) => Dependency = dependency;
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal sealed partial class Container
{
    private Container() {}
    
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultTransientScope
    {
        [UserDefinedConstructorParametersInjection(typeof(Dependency))]
        private void DIE_ConstrParams_Dependency(OtherDependency otherDependency, out int number) => number = otherDependency.Number;
    }
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = await container.Create().ConfigureAwait(false);
        Assert.Equal(69, instance.Dependency.Number);
    }
}