using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Abstraction.Interface.CollectionWithoutChoice;

internal interface IInterface {}

internal class SubClassA : IInterface {}

internal class SubClassB : IInterface {}

[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instances = container.Create();
        Assert.True(instances.Count == 2);
    }
}