using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Abstraction.AbstractClass.Collection;

internal abstract class Class {}

internal class SubClassA : Class {}

internal class SubClassB : Class {}

internal class SubClassC : Class {}

[ImplementationCollectionChoice(typeof(Class), typeof(SubClassA), typeof(SubClassB))]
[CreateFunction(typeof(IReadOnlyList<Class>), "Create")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instances = container.Create();
        Assert.True(instances.Count == 2);
        Assert.True(instances[0].GetType() == typeof(SubClassA) && instances[1].GetType() == typeof(SubClassB)
        || instances[0].GetType() == typeof(SubClassB) && instances[1].GetType() == typeof(SubClassA));
    }
}