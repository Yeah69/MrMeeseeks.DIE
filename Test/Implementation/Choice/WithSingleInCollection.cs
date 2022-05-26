using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Implementation.Choice.WithSingleInCollection;

internal class Class {}

internal class SubClassA : Class {}

internal class SubClassB : Class {}

[ImplementationChoice(typeof(Class), typeof(SubClassA))]
[ImplementationCollectionChoice(typeof(Class), typeof(SubClassB))]
[CreateFunction(typeof(Class), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var instance = container.Create();
        Assert.IsType<SubClassA>(instance);
    }
}