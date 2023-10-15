using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Decorator.ContainerInstance;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal class DependencyA : IInterface, IContainerInstance
{
    public IInterface Decorated => this;
}

internal class DependencyB : IInterface, IContainerInstance
{
    public IInterface Decorated => this;
}

internal class Decorator : IInterface, IDecorator<IInterface>
{
    public Decorator(IInterface decoratedContainerInstance) => 
        Decorated = decoratedContainerInstance;

    public IInterface Decorated { get; }
}

[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
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
        var collection = container.Create();
        
        Assert.NotEqual(collection[0], collection[1]);
        Assert.NotEqual(collection[0].Decorated, collection[1].Decorated);
        Assert.NotEqual(collection[0], collection[0].Decorated);
        Assert.NotEqual(collection[1], collection[1].Decorated);
        Assert.IsType<Decorator>(collection[0]);
        Assert.IsType<Decorator>(collection[1]);
        Assert.True(typeof(DependencyA) == collection[0].Decorated.GetType() && typeof(DependencyB) == collection[1].Decorated.GetType()
            || typeof(DependencyB) == collection[0].Decorated.GetType() && typeof(DependencyA) == collection[1].Decorated.GetType());
    }
}