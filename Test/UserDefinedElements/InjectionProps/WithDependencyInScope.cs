using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionProps.WithDependencyInScope;

internal class Dependency
{
    public int Number { get; init; }
}

internal class OtherDependency
{
    public int Number => 69;
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
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.Equal(69, instance.Dependency.Number);
    }
}