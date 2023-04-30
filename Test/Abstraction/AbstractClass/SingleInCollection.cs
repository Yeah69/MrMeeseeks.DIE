using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Abstraction.AbstractClass.SingleInCollection;

internal abstract class Class {}

internal class SubClassA : Class {}

internal class SubClassB : Class {}

[ImplementationCollectionChoice(typeof(Class), typeof(SubClassA))]
[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<SubClassA>(instance);
    }
}