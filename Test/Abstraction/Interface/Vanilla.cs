using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Abstraction.Interface.Vanilla;

internal interface IInterface {}

internal class SubClassA : IInterface {}

internal class SubClassB : IInterface {}

[ImplementationChoice(typeof(IInterface), typeof(SubClassA))]
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