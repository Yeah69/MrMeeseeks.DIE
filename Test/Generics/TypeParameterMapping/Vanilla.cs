using System.Threading.Tasks;
using Xunit;
// ReSharper disable once CheckNamespace
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.TypeParameterMapping.Vanilla;

internal sealed class Dependency<T0>;

[CreateFunction(typeof(Dependency<>), "Create")]
internal sealed partial class Container<[GenericParameterMapping(typeof(Dependency<>), "T0")] TA>;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container<int>.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Dependency<int>>(instance);
    }
}