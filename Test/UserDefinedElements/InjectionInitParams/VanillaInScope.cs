using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionInitParams.VanillaInScope;

internal sealed class Dependency
{
    public int Number { get; private set; }

    internal void Initialize(int number) => Number = number;
}

internal sealed class ScopeRoot : IScopeRoot
{
    public Dependency Dependency { get; }

    internal ScopeRoot(
        Dependency dependency) => Dependency = dependency;
}

[Initializer(typeof(Dependency), nameof(Dependency.Initialize))]
[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container
{
    
    
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultScope
    {
        [UserDefinedInitializerParametersInjection(typeof(Dependency))]
        private void DIE_InitParams_Dependency(out int number) => number = 69;
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