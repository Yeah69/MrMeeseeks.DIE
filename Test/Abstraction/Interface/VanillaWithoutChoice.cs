using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Abstraction.Interface.VanillaWithoutChoice;

internal interface IInterface {}

internal class SubClass : IInterface {}

[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = container.Create();
        Assert.IsType<SubClass>(instance);
    }
}