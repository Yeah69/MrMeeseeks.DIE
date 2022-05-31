using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Implementation.Choice.Collection;

internal class Class {}

internal class SubClassA : Class {}

internal class SubClassB : Class {}

[ImplementationCollectionChoice(typeof(Class), typeof(SubClassA), typeof(SubClassB))]
[CreateFunction(typeof(IReadOnlyList<Class>), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instances = container.Create();
        Assert.True(instances.Count == 2);
        Assert.True(instances[0].GetType() == typeof(SubClassA) && instances[1].GetType() == typeof(SubClassB)
        || instances[0].GetType() == typeof(SubClassB) && instances[1].GetType() == typeof(SubClassA));
    }
}