using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.BaseClassing.Class.InheritedContainerInstanceAttribute;

internal sealed class Class;

[ContainerInstanceImplementationAggregation(typeof(Class))]
internal abstract class ContainerBase;

[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container : ContainerBase;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instanceA = container.Create();
        var instanceB = container.Create();
        Assert.Same(instanceA, instanceB);
    }
}