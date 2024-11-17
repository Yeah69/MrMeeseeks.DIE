using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.InterfaceInterception.Decorator.ContainerInstance;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal sealed class DependencyA : IInterface, IContainerInstance
{
    public IInterface Decorated => this;
}

internal sealed class DependencyB : IInterface, IContainerInstance
{
    public IInterface Decorated => this;
}

internal sealed class Decorator : IInterface, IDecorator<IInterface>
{
    public Decorator(IInterface decoratedContainerInstance) => 
        Decorated = decoratedContainerInstance;

    public IInterface Decorated { get; }
}

[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var collection = container.Create();
        
        Assert.NotEqual(collection[0], collection[1]);
        Assert.NotEqual(collection[0].Decorated, collection[1].Decorated);
        Assert.NotEqual(collection[0], collection[0].Decorated);
        Assert.NotEqual(collection[1], collection[1].Decorated);
        Assert.IsType<Decorator>(collection[0]);
        Assert.IsType<Decorator>(collection[1]);
        Assert.True(collection[0].Decorated is DependencyA && collection[1].Decorated is DependencyB || collection[0].Decorated is DependencyB && collection[1].Decorated is DependencyA);
    }
}