using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionProps.VanillaInScope;

internal class Dependency
{
    public int Number { get; init; }
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
        private void DIE_Props_Dependency(out int Number) => Number = 69;
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