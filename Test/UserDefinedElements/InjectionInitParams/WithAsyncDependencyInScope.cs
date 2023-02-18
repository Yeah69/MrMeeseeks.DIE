using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionInitParams.WithAsyncDependencyInScope;

internal class Dependency
{
    public int Number { get; private set; }

    internal void Initialize(int number) => Number = number;
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

[Initializer(typeof(Dependency), nameof(Dependency.Initialize))]
[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container
{
    private sealed partial class DIE_DefaultScope
    {
        [UserDefinedInitializerParametersInjection(typeof(Dependency))]
        private void DIE_InitParams_Dependency(OtherDependency otherDependency, out int number) => number = otherDependency.Number;
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