using Xunit;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.RangedInstances;

internal sealed class Class<T> : IContainerInstance;

[CreateFunction(typeof(Class<>), "Create")]
[CreateFunction(typeof(Class<int>), "CreateInt")]
[CreateFunction(typeof(Class<string>), "CreateString")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instanceInt0 = container.Create<int>();
        var instanceInt1 = container.CreateInt();
        var instanceInt2 = container.Create<int>();
        var instanceString0 = container.Create<string>();
        var instanceString1 = container.CreateString();
        var instanceString2 = container.Create<string>();

        Assert.Same(instanceInt0, instanceInt1);
        Assert.Same(instanceInt1, instanceInt2);
        Assert.Same(instanceString0, instanceString1);
        Assert.Same(instanceString1, instanceString2);
        Assert.NotSame(instanceInt0, instanceString0);
    }
}