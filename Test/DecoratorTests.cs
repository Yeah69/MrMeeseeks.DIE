using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Test;
using Xunit;

[assembly:DecoratorSequenceChoice(typeof(IDecoratedMulti), typeof(DecoratorMultiA), typeof(DecoratorMultiB))]

namespace MrMeeseeks.DIE.Test;

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

[MultiContainer(typeof(IDecoratedNormal))]
internal partial class DecoratorNormalContainer
{
    
}

public partial class DecoratorTests
{
    [Fact]
    public void Normal()
    {
        using var container = new DecoratorNormalContainer();
        var decorated = container.Create0();
        Assert.NotEqual(decorated, decorated.Decorated);
        Assert.IsType<DecoratorNormal>(decorated);
        Assert.IsType<DecoratorNormalBasis>(decorated.Decorated);
    }
}

internal interface IDecoratedContainerInstance
{
    IDecoratedContainerInstance Decorated { get; }
}

internal class DecoratorContainerInstanceBasis : IDecoratedContainerInstance, IContainerInstance
{
    public IDecoratedContainerInstance Decorated => this;
}

internal class DecoratorContainerInstance : IDecoratedContainerInstance, IDecorator<IDecoratedContainerInstance>
{
    public DecoratorContainerInstance(IDecoratedContainerInstance decoratedContainerInstance) => 
        Decorated = decoratedContainerInstance;

    public IDecoratedContainerInstance Decorated { get; }
}

[MultiContainer(typeof(IDecoratedContainerInstance))]
internal partial class DecoratorContainerInstanceContainer
{
    
}

public partial class DecoratorTests
{
    [Fact]
    public void ContainerInstance()
    {
        using var container = new DecoratorContainerInstanceContainer();
        var decorated = container.Create0();
        Assert.NotEqual(decorated, decorated.Decorated);
        Assert.IsType<DecoratorContainerInstance>(decorated);
        Assert.IsType<DecoratorContainerInstanceBasis>(decorated.Decorated);
        
        var decoratedNextReference = container.Create0();
        Assert.Equal(decorated, decoratedNextReference);
        Assert.Equal(decorated.Decorated, decoratedNextReference.Decorated);
    }
}

internal interface IDecoratedScopeRoot
{
    IDecoratedScopeRootDependency Dependency { get; }
    IDecoratedScopeRoot Decorated { get; }
}

internal interface IDecoratedScopeRootDependency {}

internal class DecoratedScopeRootDependency : IDecoratedScopeRootDependency, IScopeInstance {}

internal class DecoratorScopeRootBasis : IDecoratedScopeRoot, IScopeRoot, IScopeInstance
{
    public IDecoratedScopeRootDependency Dependency { get; }

    public IDecoratedScopeRoot Decorated => this;

    public DecoratorScopeRootBasis(
        IDecoratedScopeRootDependency dependency) =>
        Dependency = dependency;
}

internal class DecoratorScopeRoot : IDecoratedScopeRoot, IDecorator<IDecoratedScopeRoot>
{
    public DecoratorScopeRoot(IDecoratedScopeRoot decorated, IDecoratedScopeRootDependency dependency)
    {
        Decorated = decorated;
        Dependency = dependency;
    }

    public IDecoratedScopeRootDependency Dependency { get; }
    public IDecoratedScopeRoot Decorated { get; }
}

[MultiContainer(typeof(IDecoratedScopeRoot))]
internal partial class DecoratorScopeRootContainer
{
    
}

public partial class DecoratorTests
{
    [Fact]
    public void ScopeRoot()
    {
        using var container = new DecoratorScopeRootContainer();
        var decorated = container.Create0();
        Assert.NotEqual(decorated, decorated.Decorated);
        Assert.Equal(decorated.Dependency, decorated.Decorated.Dependency);
        Assert.IsType<DecoratorScopeRoot>(decorated);
        Assert.IsType<DecoratorScopeRootBasis>(decorated.Decorated);
        
        // There is yet no way to check scopes externally
        var next = container.Create0();
        Assert.NotEqual(decorated, next);
        Assert.NotEqual(decorated.Dependency, next.Dependency);
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

[MultiContainer(typeof(IDecoratedMulti))]
internal partial class DecoratorMultiContainer
{
    
}

public partial class DecoratorTests
{
    [Fact]
    public void Multi()
    {
        using var container = new DecoratorMultiContainer();
        var decorated = container.Create0();
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

[MultiContainer(typeof(IReadOnlyList<IDecoratedList>))]
internal partial class DecoratorListContainer
{
    
}

public partial class DecoratorTests
{
    [Fact]
    public void List()
    {
        using var container = new DecoratorListContainer();
        var decorated = container.Create0();
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