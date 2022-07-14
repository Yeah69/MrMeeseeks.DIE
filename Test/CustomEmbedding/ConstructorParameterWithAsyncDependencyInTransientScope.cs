using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CustomEmbedding.ConstructorParameterWithAsyncDependencyInTransientScope;

internal class Dependency
{
    public int Number { get; }

    internal Dependency(int number) => Number = number;
}

internal class OtherDependency : IValueTaskTypeInitializer
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
    private sealed partial class DIE_DefaultTransientScope
    {
        [CustomConstructorParameter(typeof(Dependency))]
        private void DIE_ConstrParam_Dependency(OtherDependency otherDependency, out int number) => number = otherDependency.Number;
    }
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = await container.CreateAsync().ConfigureAwait(false);
        Assert.Equal(69, instance.Dependency.Number);
    }
}