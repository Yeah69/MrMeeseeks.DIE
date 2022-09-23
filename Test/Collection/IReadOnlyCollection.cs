using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Collection.IReadOnlyCollection;

internal interface IInterface {}

internal class ClassA : IInterface {}

internal class ClassB : IInterface {}

internal class ClassC : IInterface {}

[CreateFunction(typeof(IReadOnlyCollection<IInterface>), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var collection = container.Create();
        Assert.Equal(3, collection.Count);
    }
}