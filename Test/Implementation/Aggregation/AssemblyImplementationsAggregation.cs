using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.TestNotInternalsVisibleToChild.Public;
using Xunit;

namespace MrMeeseeks.DIE.Test.Implementation.Aggregation.AssemblyImplementationsAggregation;

[FilterAllImplementationsAggregation]
[AssemblyImplementationsAggregation(typeof(MrMeeseeks.DIE.TestNotInternalsVisibleToChild.AssemblyInfo))]
[ConstructorChoice(typeof(Parent.ClassToo))]
[CreateFunction(typeof(Parent.ClassToo), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = container.Create();
        Assert.IsType<Parent.ClassToo>(instance);
    }
}