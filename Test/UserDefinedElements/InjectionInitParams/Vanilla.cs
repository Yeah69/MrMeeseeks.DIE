using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.UserDefinedElements.InjectionInitParams.Vanilla;

internal sealed class Dependency
{
    public int Number { get; private set; }

    internal void Initialize(int number) => Number = number;
}

[Initializer(typeof(Dependency), nameof(Dependency.Initialize))]
[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    
    
    [UserDefinedInitializerParametersInjection(typeof(Dependency))]
    private void DIE_InitParams_Dependency(out int number) => number = 69;
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.Equal(69, instance.Number);
    }
}