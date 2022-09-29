using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionProps.WithDependencyInTransientScope;

internal class Dependency
{
    public int Number { get; init; }
}

internal class OtherDependency
{
    public int Number => 69;
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
        private void DIE_Props_Dependency(OtherDependency otherDependency, out int Number) => Number = otherDependency.Number;
    }
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = container.Create();
        Assert.Equal(69, instance.Dependency.Number);
    }
}