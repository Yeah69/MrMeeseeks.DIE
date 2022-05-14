using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Implementation.Choice.Vanilla;

internal class Class {}

internal class SubClass : Class {}

[ImplementationChoice(typeof(Class), typeof(SubClass))]
[CreateFunction(typeof(Class), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var instance = container.Create();
        Assert.IsType<SubClass>(instance);
    }
}