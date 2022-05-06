using System;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.ContainerInstanceWithDifferentGenericParameter;

internal class Class<T0> : IContainerInstance { }

[CreateFunction(typeof(Class<int>), "Create")]
[CreateFunction(typeof(Class<string>), "CreateString")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var instance0 = container.Create();
        var instance1 = container.CreateString();
        Assert.NotSame(instance0, instance1);
    }
}