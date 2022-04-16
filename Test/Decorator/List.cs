using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Decorator.List;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal class DependencyA : IInterface
{
    public IInterface Decorated => this;
}

internal class DependencyB : IInterface
{
    public IInterface Decorated => this;
}

internal class Decorator : IInterface, IDecorator<IInterface>
{
    public Decorator(IInterface decorated) => 
        Decorated = decorated;

    public IInterface Decorated { get; }
}

[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
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
        var decoratedOfA = decorated[0];
        var decoratedOfB = decorated[1];
        var decoratedBasisA = decoratedOfA.Decorated;
        var decoratedBasisB = decoratedOfB.Decorated;
        Assert.NotEqual(decoratedOfA, decoratedBasisA);
        Assert.NotEqual(decoratedOfB, decoratedBasisB);
        Assert.NotEqual(decoratedOfA, decoratedOfB);
        Assert.NotEqual(decoratedBasisA, decoratedBasisB);
        Assert.IsType<Decorator>(decoratedOfA);
        Assert.IsType<Decorator>(decoratedOfB);
        Assert.IsType<DependencyA>(decoratedBasisA);
        Assert.IsType<DependencyB>(decoratedBasisB);
    }
}