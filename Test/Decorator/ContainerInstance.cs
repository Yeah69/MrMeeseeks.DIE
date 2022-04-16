using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Decorator.ContainerInstance;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal class Dependency : IInterface, IContainerInstance
{
    public IInterface Decorated => this;
}

internal class Decorator : IInterface, IDecorator<IInterface>
{
    public Decorator(IInterface decoratedContainerInstance) => 
        Decorated = decoratedContainerInstance;

    public IInterface Decorated { get; }
}

[CreateFunction(typeof(IInterface), "Create")]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var decorated = container.Create();
        Assert.NotEqual(decorated, decorated.Decorated);
        Assert.IsType<Decorator>(decorated);
        Assert.IsType<Dependency>(decorated.Decorated);
        
        var decoratedNextReference = container.Create();
        Assert.Equal(decorated, decoratedNextReference);
        Assert.Equal(decorated.Decorated, decoratedNextReference.Decorated);
    }
}