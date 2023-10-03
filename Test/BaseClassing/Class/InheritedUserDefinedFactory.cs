using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.BaseClassing.Class.InheritedUserDefinedFactory;

internal abstract class ContainerBase
{
    protected int DIE_Factory_Int => 69;
}

[CreateFunction(typeof(int), "Create")]
internal sealed partial class Container : ContainerBase
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var number = container.Create();
        Assert.Equal(69, number);
    }
}