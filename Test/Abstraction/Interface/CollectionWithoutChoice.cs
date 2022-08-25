using System.Collections.Generic;
using System.Threading.Tasks;
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
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instances = container.Create();
        Assert.True(instances.Count == 2);
    }
}