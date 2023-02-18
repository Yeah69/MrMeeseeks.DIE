using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionProps.WithAsyncDependencyInScope;

internal class Dependency
{
    public int Number { get; init; }
}

internal class OtherDependency : IValueTaskInitializer
{
    public int Number => 69;
    public ValueTask InitializeAsync() => new (Task.CompletedTask);
}

internal class ScopeRoot : IScopeRoot
{
    public Dependency Dependency { get; }

    internal ScopeRoot(
        Dependency dependency) => Dependency = dependency;
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container
{
    private sealed partial class DIE_DefaultScope
    {
        [UserDefinedPropertiesInjection(typeof(Dependency))]
        private void DIE_Props_Dependency(OtherDependency otherDependency, out int Number) => Number = otherDependency.Number;
    }
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = new Container();
        var instance = await container.Create().ConfigureAwait(false);
        Assert.Equal(69, instance.Dependency.Number);
    }
}