using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.TestInternalsVisibleToChild.Internal;
using Xunit;

namespace MrMeeseeks.DIE.Test.Implementation.Aggregation.InternalsVisibleTo;

[ConstructorChoice(typeof(Parent.ClassToo))]
[CreateFunction(typeof(Parent.ClassToo), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Parent.ClassToo>(instance);
    }
}