using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Configuration.ContainerInstanceWithDifferentGenericParameter;

// ReSharper disable once UnusedTypeParameter
internal class Class<T0> : IContainerInstance { }

[CreateFunction(typeof(Class<int>), "Create")]
[CreateFunction(typeof(Class<string>), "CreateString")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance0 = container.Create();
        var instance1 = container.CreateString();
        Assert.NotSame(instance0, instance1);
    }
}