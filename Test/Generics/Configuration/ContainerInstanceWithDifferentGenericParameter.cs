using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.ContainerInstanceWithDifferentGenericParameter;

internal class Class<T0> : IContainerInstance { }

[CreateFunction(typeof(Class<int>), "Create")]
[CreateFunction(typeof(Class<string>), "CreateString")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance0 = container.Create();
        var instance1 = container.CreateString();
        Assert.NotSame(instance0, instance1);
    }
}