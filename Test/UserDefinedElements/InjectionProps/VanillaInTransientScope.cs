using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionProps.VanillaInTransientScope;

internal class Dependency
{
    public int Number { get; init; }
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