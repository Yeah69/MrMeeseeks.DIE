using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.BaseClassing.Class.InheritedContainerInstanceAttribute;

internal class Class;

[ContainerInstanceImplementationAggregation(typeof(Class))]
internal abstract class ContainerBase;

[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container : ContainerBase;

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