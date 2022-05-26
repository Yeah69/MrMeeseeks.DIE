using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.TestInternalsVisibleToChild.Internal;
using Xunit;

namespace MrMeeseeks.DIE.Test.Implementation.Aggregation.InternalsVisibleTo;

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