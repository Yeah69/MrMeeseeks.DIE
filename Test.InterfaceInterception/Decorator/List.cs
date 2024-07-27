using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.InterfaceInterception.Decorator.List;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal sealed class DependencyA : IInterface
{
    public IInterface Decorated => this;
}

internal sealed class DependencyB : IInterface
{
    public IInterface Decorated => this;
}

internal sealed class Decorator : IInterface, IDecorator<IInterface>
{
    public Decorator(IInterface decorated) => 
        Decorated = decorated;

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
        Assert.True(decoratedBasisA is DependencyA && decoratedBasisB is DependencyB || decoratedBasisA is DependencyB && decoratedBasisB is DependencyA);
    }
}