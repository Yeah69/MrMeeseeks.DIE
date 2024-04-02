using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.ViaFactory;

internal sealed class Dependency : IContainerInstance;

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    public Dependency? FromFactory { get; private set; }
    private Dependency DIE_Factory()
    {
        FromFactory = new();
        return FromFactory;
    }
}

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Dependency>(instance);
        Assert.Same(instance, container.FromFactory);
        var anotherCreateCall = container.Create();
        Assert.Same(instance, anotherCreateCall);
    }
}