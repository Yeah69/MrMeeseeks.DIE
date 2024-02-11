using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.TestInternalsVisibleToChild.Internal;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Implementation.Aggregation.InternalsVisibleTo;

[ConstructorChoice(typeof(Parent.ClassToo))]
[CreateFunction(typeof(Parent.ClassToo), "Create")]
internal sealed partial class Container { }

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