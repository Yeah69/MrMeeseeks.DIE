using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;
// ReSharper disable UnusedParameter.Local

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.InitializedInstances.AsReturnedType;

internal sealed class Dependency;

[InitializedInstances(typeof(Dependency))]
[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var referenceA = container.Create();
        var referenceB = container.Create();
        Assert.Same(referenceA, referenceB);
    }
}