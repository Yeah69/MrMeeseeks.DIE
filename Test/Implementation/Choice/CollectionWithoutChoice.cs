using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Implementation.Choice.CollectionWithoutChoice;

internal class Class {}

internal class SubClassA : Class {}

internal class SubClassB : Class {}

[CreateFunction(typeof(IReadOnlyList<Class>), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instances = container.Create();
        Assert.True(instances.Count == 3);
    }
}