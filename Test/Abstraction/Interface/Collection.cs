using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Abstraction.Interface.Collection;

internal interface IInterface {}

internal class SubClassA : IInterface {}

internal class SubClassB : IInterface {}

internal class SubClassC : IInterface {}

[ImplementationCollectionChoice(typeof(IInterface), typeof(SubClassA), typeof(SubClassB))]
[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
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
        var instances = container.Create();
        Assert.True(instances.Count == 2);
        Assert.True(instances[0].GetType() == typeof(SubClassA) && instances[1].GetType() == typeof(SubClassB)
        || instances[0].GetType() == typeof(SubClassB) && instances[1].GetType() == typeof(SubClassA));
    }
}