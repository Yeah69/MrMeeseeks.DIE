using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.TestNotInternalsVisibleToChild.Public;
using MrMeeseeks.DIE.TestNotInternalsVisibleToChild;
using Xunit;

namespace MrMeeseeks.DIE.Test.Implementation.Aggregation.AssemblyImplementationsAggregation;

[FilterAllImplementationsAggregation]
[AssemblyImplementationsAggregation(typeof(AssemblyInfo))]
[ConstructorChoice(typeof(Parent.ClassToo))]
[CreateFunction(typeof(Parent.ClassToo), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var instance = container.Create();
        Assert.IsType<Parent.ClassToo>(instance);
    }
}