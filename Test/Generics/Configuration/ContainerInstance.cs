using System;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.ContainerInstance;

internal class Class<T0> : IContainerInstance { }

[CreateFunction(typeof(Class<int>), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var instance0 = container.Create();
        var instance1 = container.Create();
        Assert.Same(instance0, instance1);
    }
}