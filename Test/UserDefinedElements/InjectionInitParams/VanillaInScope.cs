using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionInitParams.VanillaInScope;

internal class Dependency
{
    public int Number { get; private set; }

    internal void Initialize(int number) => Number = number;
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
        private void DIE_InitParams_Dependency(out int number) => number = 69;
    }
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = container.Create().Dependency;
        Assert.Equal(69, instance.Number);
    }
}