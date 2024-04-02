using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Abstraction.AbstractClass.WithSingleInCollection;

internal abstract class Class;

internal sealed class SubClassA : Class;

internal sealed class SubClassB : Class;

[ImplementationChoice(typeof(Class), typeof(SubClassA))]
[ImplementationCollectionChoice(typeof(Class), typeof(SubClassB))]
[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<SubClassA>(instance);
    }
}