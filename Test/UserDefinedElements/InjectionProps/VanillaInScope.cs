using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionProps.VanillaInScope;

internal sealed class Dependency
{
    public int Number { get; init; }
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
        [UserDefinedPropertiesInjection(typeof(Dependency))]
        // ReSharper disable once InconsistentNaming
        private void DIE_Props_Dependency(out int Number) => Number = 69;
    }
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create().Dependency;
        Assert.Equal(69, instance.Number);
    }
}