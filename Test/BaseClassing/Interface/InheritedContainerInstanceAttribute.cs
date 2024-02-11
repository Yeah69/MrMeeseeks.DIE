using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.BaseClassing.Interface.InheritedContainerInstanceAttribute;

internal class Class {}

[ContainerInstanceImplementationAggregation(typeof(Class))]
internal interface IContainerBase { }

[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container : IContainerBase { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instanceA = container.Create();
        var instanceB = container.Create();
        Assert.Same(instanceA, instanceB);
    }
}