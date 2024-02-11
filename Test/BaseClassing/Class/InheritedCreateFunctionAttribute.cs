using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.BaseClassing.Class.InheritedCreateFunctionAttribute;

internal class Class {}

[CreateFunction(typeof(Class), "Create")]
internal abstract class ContainerBase { }

internal sealed partial class Container : ContainerBase { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.NotNull(instance);
    }
}