using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Bugs.CustomFactoryForImplementationType;

internal class Dependency
{
    internal int Value { get; init; } = 23;
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    private Dependency DIE_Factory_Dependency => new() { Value = 69 };
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.Equal(69, instance.Value);
    }
}