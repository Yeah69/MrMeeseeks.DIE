using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Decorator.Normal;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal class Dependency : IInterface
{
    public IInterface Decorated => this;
}

internal class Decorator : IInterface, IDecorator<IInterface>
{
    public Decorator(IInterface decoratedNormal) => 
        Decorated = decoratedNormal;

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
    }
}