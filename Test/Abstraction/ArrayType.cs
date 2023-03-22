using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Abstraction.ArrayType;

internal interface IInterface {}

internal class DependencyA : IInterface {}

internal class DependencyB : IInterface {}

internal class DependencyC : IInterface {}

[CreateFunction(typeof(IInterface[]), "Create")]
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
        Assert.Equal(3, instance.Length);
        Assert.IsAssignableFrom<IInterface>(instance[0]);
        Assert.IsAssignableFrom<IInterface>(instance[0]);
        Assert.IsAssignableFrom<IInterface>(instance[0]);
    }
}