using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Abstraction.Interface.SingleInCollection;

internal interface IInterface {}

internal class SubClassA : IInterface {}

internal class SubClassB : IInterface {}

[ImplementationCollectionChoice(typeof(IInterface), typeof(SubClassA))]
[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.IsType<SubClassA>(instance);
    }
}