using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.BaseClassing.Interface.InheritedContainerInstanceAttribute;

internal sealed class Class;

[ContainerInstanceImplementationAggregation(typeof(Class))]
internal interface IContainerBase;

[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container : IContainerBase;

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