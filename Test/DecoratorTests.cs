using System.Collections.Generic;
using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Sample;
using Xunit;

[assembly:SingleInstance(typeof(ISingleInstance))]
[assembly:ScopedInstance(typeof(IScopedInstance))]
[assembly:ScopeRoot(typeof(IScopeRoot))]
[assembly:Transient(typeof(ITransient))]
[assembly:Decorator(typeof(IDecorator<>))]

namespace MrMeeseeks.DIE.Sample;

internal interface IDecoratedNormal
{
    IDecoratedNormal Decorated { get; }
}

internal class DecoratorNormalBasis : IDecoratedNormal
{
    public IDecoratedNormal Decorated => this;
}

internal class DecoratorNormal : IDecoratedNormal, IDecorator<IDecoratedNormal>
{
    public DecoratorNormal(IDecoratedNormal decoratedNormal) => 
        Decorated = decoratedNormal;

    public IDecoratedNormal Decorated { get; }
}

internal partial class DecoratorNormalContainer : IContainer<IDecoratedNormal>
{
    
}

public partial class DecoratorTests
{
    [Fact]
    public void Normal()
    {
        using var container = new DecoratorNormalContainer();
        var decorated = ((IContainer<IDecoratedNormal>) container).Resolve();
        Assert.NotEqual(decorated, decorated.Decorated);
        Assert.IsType<DecoratorNormal>(decorated);
        Assert.IsType<DecoratorNormalBasis>(decorated.Decorated);
    }
}

internal interface IDecoratedSingleInstance
{
    IDecoratedSingleInstance Decorated { get; }
}

internal class DecoratorSingleInstanceBasis : IDecoratedSingleInstance, ISingleInstance
{
    public IDecoratedSingleInstance Decorated => this;
}

internal class DecoratorSingleInstance : IDecoratedSingleInstance, IDecorator<IDecoratedSingleInstance>
{
    public DecoratorSingleInstance(IDecoratedSingleInstance decoratedSingleInstance) => 
        Decorated = decoratedSingleInstance;

    public IDecoratedSingleInstance Decorated { get; }
}

internal partial class DecoratorSingleInstanceContainer : IContainer<IDecoratedSingleInstance>
{
    
}

public partial class DecoratorTests
{
    [Fact]
    public void SingleInstance()
    {
        using var container = new DecoratorSingleInstanceContainer();
        var decorated = ((IContainer<IDecoratedSingleInstance>) container).Resolve();
        Assert.NotEqual(decorated, decorated.Decorated);
        Assert.IsType<DecoratorSingleInstance>(decorated);
        Assert.IsType<DecoratorSingleInstanceBasis>(decorated.Decorated);
        
        var decoratedNextReference = ((IContainer<IDecoratedSingleInstance>) container).Resolve();
        Assert.Equal(decorated, decoratedNextReference);
        Assert.Equal(decorated.Decorated, decoratedNextReference.Decorated);
    }
}

internal interface IDecoratedScopeRoot
{
    IDecoratedScopeRoot Decorated { get; }
}

internal class DecoratorScopeRootBasis : IDecoratedScopeRoot, IScopeRoot
{
    public IDecoratedScopeRoot Decorated => this;
}

internal class DecoratorScopeRoot : IDecoratedScopeRoot, IDecorator<IDecoratedScopeRoot>
{
    public DecoratorScopeRoot(IDecoratedScopeRoot decorated) => 
        Decorated = decorated;

    public IDecoratedScopeRoot Decorated { get; }
}

internal partial class DecoratorScopeRootContainer : IContainer<IDecoratedScopeRoot>
{
    
}

public partial class DecoratorTests
{
    [Fact]
    public void ScopeRoot()
    {
        using var container = new DecoratorScopeRootContainer();
        var decorated = ((IContainer<IDecoratedScopeRoot>) container).Resolve();
        Assert.NotEqual(decorated, decorated.Decorated);
        Assert.IsType<DecoratorScopeRoot>(decorated);
        Assert.IsType<DecoratorScopeRootBasis>(decorated.Decorated);
        
        // There is yet no way to check scopes externally
        Assert.True(false);
    }
}

internal interface IDecoratedMulti
{
    IDecoratedMulti Decorated { get; }
}

internal class DecoratorMultiBasis : IDecoratedMulti
{
    public IDecoratedMulti Decorated => this;
}

internal class DecoratorMultiA : IDecoratedMulti, IDecorator<IDecoratedMulti>
{
    public DecoratorMultiA(IDecoratedMulti decorated) => 
        Decorated = decorated;

    public IDecoratedMulti Decorated { get; }
}

internal class DecoratorMultiB : IDecoratedMulti, IDecorator<IDecoratedMulti>
{
    public DecoratorMultiB(IDecoratedMulti decorated) => 
        Decorated = decorated;

    public IDecoratedMulti Decorated { get; }
}

internal partial class DecoratorMultiContainer : IContainer<IDecoratedMulti>
{
    
}

public partial class DecoratorTests
{
    [Fact]
    public void Multi()
    {
        using var container = new DecoratorMultiContainer();
        var decorated = ((IContainer<IDecoratedMulti>) container).Resolve();
        var decoratedB = decorated;
        var decoratedA = decorated.Decorated;
        var decoratedBasis = decoratedA.Decorated;
        Assert.NotEqual(decoratedBasis, decoratedA);
        Assert.NotEqual(decoratedBasis, decoratedB);
        Assert.NotEqual(decoratedA, decoratedB);
        Assert.IsType<DecoratorMultiBasis>(decoratedBasis);
        Assert.IsType<DecoratorMultiA>(decoratedA);
        Assert.IsType<DecoratorMultiB>(decoratedB);
    }
}

internal interface IDecoratedList
{
    IDecoratedList Decorated { get; }
}

internal class DecoratedListBasisA : IDecoratedList
{
    public IDecoratedList Decorated => this;
}

internal class DecoratedListBasisB : IDecoratedList
{
    public IDecoratedList Decorated => this;
}

internal class DecoratorList : IDecoratedList, IDecorator<IDecoratedList>
{
    public DecoratorList(IDecoratedList decorated) => 
        Decorated = decorated;

    public IDecoratedList Decorated { get; }
}

internal partial class DecoratorListContainer : IContainer<IReadOnlyList<IDecoratedList>>
{
    
}

public partial class DecoratorTests
{
    [Fact]
    public void List()
    {
        using var container = new DecoratorListContainer();
        var decorated = ((IContainer<IReadOnlyList<IDecoratedList>>) container).Resolve();
        var decoratedOfA = decorated[0];
        var decoratedOfB = decorated[1];
        var decoratedBasisA = decoratedOfA.Decorated;
        var decoratedBasisB = decoratedOfB.Decorated;
        Assert.NotEqual(decoratedOfA, decoratedBasisA);
        Assert.NotEqual(decoratedOfB, decoratedBasisB);
        Assert.NotEqual(decoratedOfA, decoratedOfB);
        Assert.NotEqual(decoratedBasisA, decoratedBasisB);
        Assert.IsType<DecoratorList>(decoratedOfA);
        Assert.IsType<DecoratorList>(decoratedOfB);
        Assert.IsType<DecoratedListBasisA>(decoratedBasisA);
        Assert.IsType<DecoratedListBasisB>(decoratedBasisB);
    }
}