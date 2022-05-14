using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Implementation.Choice.SingleInCollection;

internal class Class {}

internal class SubClass : Class {}

[ImplementationCollectionChoice(typeof(Class), typeof(SubClass))]
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