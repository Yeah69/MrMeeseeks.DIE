using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
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
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
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
        Assert.True(decoratedBasisA.GetType() == typeof(DependencyA) && decoratedBasisB.GetType() == typeof(DependencyB)
        || decoratedBasisA.GetType() == typeof(DependencyB) && decoratedBasisB.GetType() == typeof(DependencyA));
    }
}