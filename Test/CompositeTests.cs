using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Test;
using Xunit;

[assembly:DecoratorSequenceChoice(typeof(ICompositeDecorated), typeof(CompositeDecoratedDecoratorA), typeof(CompositeDecoratedDecoratorB))]
[assembly:DecoratorSequenceChoice(typeof(CompositeDecorated), typeof(CompositeDecoratedDecoratorB))]

namespace MrMeeseeks.DIE.Test;

internal interface ICompositeNormal
{
    IReadOnlyList<ICompositeNormal> Composites { get; }
}

internal class CompositeNormalBasisA : ICompositeNormal
{
    public IReadOnlyList<ICompositeNormal> Composites => new List<ICompositeNormal> { this };
}

internal class CompositeNormalBasisB : ICompositeNormal
{
    public IReadOnlyList<ICompositeNormal> Composites => new List<ICompositeNormal> { this };
}

internal class CompositeNormal : ICompositeNormal, IComposite<ICompositeNormal>
{
    public CompositeNormal(IReadOnlyList<ICompositeNormal> composites) => 
        Composites = composites;

    public IReadOnlyList<ICompositeNormal> Composites { get; }
}

[CreateFunction(typeof(ICompositeNormal), "CreateDep")]
[CreateFunction(typeof(IReadOnlyList<ICompositeNormal>), "CreateCollection")]
internal partial class CompositeNormalContainer
{
    
}

public partial class CompositeTests
{
    [Fact]
    public void Normal()
    {
        using var container = new CompositeNormalContainer();
        var composite = container.CreateDep();
        foreach (var compositeComposite in composite.Composites)
        {
            Assert.NotEqual(composite, compositeComposite);
            var type = compositeComposite.GetType();
            Assert.True(type == typeof(CompositeNormalBasisA) || type == typeof(CompositeNormalBasisB));
        }
    }
    
    [Fact]
    public void NormalList()
    {
        using var container = new CompositeNormalContainer();
        var composites = container.CreateCollection();
        foreach (var compositeComposite in composites)
        {
            var type = compositeComposite.GetType();
            Assert.True(type == typeof(CompositeNormalBasisA) || type == typeof(CompositeNormalBasisB));
        }
        Assert.Equal(2, composites.Count);
    }
}

internal interface ICompositeContainerInstance
{
    IReadOnlyList<ICompositeContainerInstance> Composites { get; }
}

internal class CompositeContainerInstanceBasisA : ICompositeContainerInstance, IContainerInstance
{
    public IReadOnlyList<ICompositeContainerInstance> Composites => new List<ICompositeContainerInstance> { this };
}

internal class CompositeContainerInstanceBasisB : ICompositeContainerInstance, IContainerInstance
{
    public IReadOnlyList<ICompositeContainerInstance> Composites => new List<ICompositeContainerInstance> { this };
}

internal class CompositeContainerInstance : ICompositeContainerInstance, IComposite<ICompositeContainerInstance>
{
    public CompositeContainerInstance(IReadOnlyList<ICompositeContainerInstance> composites) => 
        Composites = composites;

    public IReadOnlyList<ICompositeContainerInstance> Composites { get; }
}

[CreateFunction(typeof(ICompositeContainerInstance), "CreateDep")]
[CreateFunction(typeof(IReadOnlyList<ICompositeContainerInstance>), "CreateCollection")]
internal partial class CompositeContainerInstanceContainer
{
    
}

public partial class CompositeTests
{
    [Fact]
    public void ContainerInstance()
    {
        using var container = new CompositeContainerInstanceContainer();
        var composite = container.CreateDep();
        foreach (var compositeComposite in composite.Composites)
        {
            Assert.NotEqual(composite, compositeComposite);
            var type = compositeComposite.GetType();
            Assert.True(type == typeof(CompositeContainerInstanceBasisA) || type == typeof(CompositeContainerInstanceBasisB));
        }
        var nextComposite = container.CreateDep();
        Assert.Equal(composite, nextComposite);
    }
    
    [Fact]
    public void ContainerInstanceList()
    {
        using var container = new CompositeContainerInstanceContainer();
        var composites = container.CreateCollection();
        foreach (var compositeComposite in composites)
        {
            var type = compositeComposite.GetType();
            Assert.True(type == typeof(CompositeContainerInstanceBasisA) || type == typeof(CompositeContainerInstanceBasisB));
        }
        Assert.Equal(2, composites.Count);
        var nextComposites = container.CreateCollection();
        Assert.Equal(composites[0], nextComposites[0]);
        Assert.Equal(composites[1], nextComposites[1]);
    }
}

internal interface ICompositeMixedScoping
{
    IReadOnlyList<ICompositeMixedScoping> Composites { get; }
}

internal class CompositeMixedScopingBasisA : ICompositeMixedScoping, IContainerInstance
{
    public IReadOnlyList<ICompositeMixedScoping> Composites => new List<ICompositeMixedScoping> { this };
}

internal class CompositeMixedScopingBasisB : ICompositeMixedScoping
{
    public IReadOnlyList<ICompositeMixedScoping> Composites => new List<ICompositeMixedScoping> { this };
}

internal class CompositeMixedScoping : ICompositeMixedScoping, IComposite<ICompositeMixedScoping>
{
    public CompositeMixedScoping(IReadOnlyList<ICompositeMixedScoping> composites) => 
        Composites = composites;

    public IReadOnlyList<ICompositeMixedScoping> Composites { get; }
}

[CreateFunction(typeof(ICompositeMixedScoping), "CreateDep")]
[CreateFunction(typeof(IReadOnlyList<ICompositeMixedScoping>), "CreateCollection")]
internal partial class CompositeMixedScopingContainer
{
    
}

public partial class CompositeTests
{
    [Fact]
    public void MixedScoping()
    {
        using var container = new CompositeMixedScopingContainer();
        var composite = container.CreateDep();
        foreach (var compositeComposite in composite.Composites)
        {
            Assert.NotEqual(composite, compositeComposite);
            var type = compositeComposite.GetType();
            Assert.True(type == typeof(CompositeMixedScopingBasisA) || type == typeof(CompositeMixedScopingBasisB));
        }
        var nextComposite = container.CreateDep();
        Assert.NotEqual(composite, nextComposite);
    }
    
    [Fact]
    public void MixedScopingList()
    {
        using var container = new CompositeMixedScopingContainer();
        var composites = container.CreateCollection();
        foreach (var compositeComposite in composites)
        {
            var type = compositeComposite.GetType();
            Assert.True(type == typeof(CompositeMixedScopingBasisA) || type == typeof(CompositeMixedScopingBasisB));
        }
        Assert.Equal(2, composites.Count);
        var nextComposites = container.CreateCollection();
        Assert.True(composites[0].Equals(nextComposites[0]) && !composites[1].Equals(nextComposites[1])
            || composites[1].Equals(nextComposites[1]) && !composites[0].Equals(nextComposites[0]));
    }
}

internal interface ICompositeScopeRoot
{
    IReadOnlyList<ICompositeScopeRoot> Composites { get; }
    ICompositeScopeRootDependency Dependency { get; }
}

internal interface ICompositeScopeRootDependency { }

internal class CompositeScopeRootDependency : ICompositeScopeRootDependency, IScopeInstance { }

internal class CompositeScopeRootBasisA : ICompositeScopeRoot, IScopeRoot
{
    public CompositeScopeRootBasisA(ICompositeScopeRootDependency dependency) => Dependency = dependency;

    public IReadOnlyList<ICompositeScopeRoot> Composites => new List<ICompositeScopeRoot> { this };
    public ICompositeScopeRootDependency Dependency { get; }
}

internal class CompositeScopeRootBasisB : ICompositeScopeRoot
{
    public CompositeScopeRootBasisB(ICompositeScopeRootDependency dependency) => Dependency = dependency;

    public IReadOnlyList<ICompositeScopeRoot> Composites => new List<ICompositeScopeRoot> { this };
    public ICompositeScopeRootDependency Dependency { get; }
}

internal class CompositeScopeRoot : ICompositeScopeRoot, IComposite<ICompositeScopeRoot>
{
    public CompositeScopeRoot(IReadOnlyList<ICompositeScopeRoot> composites, ICompositeScopeRootDependency dependency)
    {
        Composites = composites;
        Dependency = dependency;
    }

    public IReadOnlyList<ICompositeScopeRoot> Composites { get; }
    public ICompositeScopeRootDependency Dependency { get; }
}

[CreateFunction(typeof(ICompositeScopeRoot), "CreateDep")]
[CreateFunction(typeof(IReadOnlyList<ICompositeScopeRoot>), "CreateCollection")]
internal partial class CompositeScopeRootContainer
{
    
}

public partial class CompositeTests
{
    [Fact]
    public void ScopeRoot()
    {
        using var container = new CompositeScopeRootContainer();
        var composite = container.CreateDep();
        foreach (var compositeComposite in composite.Composites)
        {
            Assert.NotEqual(composite, compositeComposite);
            var type = compositeComposite.GetType();
            Assert.True(type == typeof(CompositeScopeRootBasisA) || type == typeof(CompositeScopeRootBasisB));
            Assert.Equal(composite.Dependency, compositeComposite.Dependency);
        }
        var next = container.CreateDep();
        Assert.NotEqual(composite.Dependency, next.Dependency);
    }
    
    [Fact]
    public void ScopeRootList()
    {
        using var container = new CompositeScopeRootContainer();
        var composites = container.CreateCollection();
        foreach (var compositeComposite in composites)
        {
            var type = compositeComposite.GetType();
            Assert.True(type == typeof(CompositeScopeRootBasisA) || type == typeof(CompositeScopeRootBasisB));
        }
        Assert.Equal(2, composites.Count);
        Assert.NotEqual(composites[0].Dependency, composites[1].Dependency);
    }
}

internal interface ICompositeDecorated
{
    IReadOnlyList<ICompositeDecorated> Composites { get; }
    ICompositeDecorated Decorated { get; }
}

internal class CompositeDecoratedDecoratorA : ICompositeDecorated, IDecorator<ICompositeDecorated>
{
    public IReadOnlyList<ICompositeDecorated> Composites => Decorated.Composites;
    public ICompositeDecorated Decorated { get; }

    public CompositeDecoratedDecoratorA(
        ICompositeDecorated decorated) =>
        Decorated = decorated;
}

internal class CompositeDecoratedDecoratorB : ICompositeDecorated, IDecorator<ICompositeDecorated>
{
    public IReadOnlyList<ICompositeDecorated> Composites => Decorated.Composites;
    public ICompositeDecorated Decorated { get; }

    public CompositeDecoratedDecoratorB(
        ICompositeDecorated decorated) =>
        Decorated = decorated;
}

internal class CompositeDecoratedBasisA : ICompositeDecorated
{
    public IReadOnlyList<ICompositeDecorated> Composites => new List<ICompositeDecorated> { this };
    public ICompositeDecorated Decorated => this;
}

internal class CompositeDecoratedBasisB : ICompositeDecorated
{
    public IReadOnlyList<ICompositeDecorated> Composites => new List<ICompositeDecorated> { this };
    public ICompositeDecorated Decorated => this;
}

internal class CompositeDecorated : ICompositeDecorated, IComposite<ICompositeDecorated>
{
    public CompositeDecorated(IReadOnlyList<ICompositeDecorated> composites) => 
        Composites = composites;

    public IReadOnlyList<ICompositeDecorated> Composites { get; }
    public ICompositeDecorated Decorated => this;
}

[CreateFunction(typeof(ICompositeDecorated), "CreateDep")]
[CreateFunction(typeof(IReadOnlyList<ICompositeDecorated>), "CreateCollection")]
internal partial class CompositeDecoratedContainer
{
    
}

public partial class CompositeTests
{
    [Fact]
    public void Decorated()
    {
        using var container = new CompositeDecoratedContainer();
        var composite = container.CreateDep();
        Assert.IsType<CompositeDecoratedDecoratorB>(composite);
        Assert.IsType<CompositeDecorated>(composite.Decorated);
        foreach (var compositeComposite in composite.Composites)
        {
            Assert.NotEqual(composite, compositeComposite);
            Assert.IsType<CompositeDecoratedDecoratorB>(compositeComposite);
            Assert.IsType<CompositeDecoratedDecoratorA>(compositeComposite.Decorated);
            var baseImpl = compositeComposite.Decorated.Decorated;
            Assert.True(baseImpl.GetType() == typeof(CompositeDecoratedBasisA) || baseImpl.GetType() == typeof(CompositeDecoratedBasisB));
        }
    }
    
    [Fact]
    public void DecoratedList()
    {
        using var container = new CompositeDecoratedContainer();
        var composites = container.CreateCollection();
        foreach (var compositeComposite in composites)
        {
            Assert.IsType<CompositeDecoratedDecoratorB>(compositeComposite);
            Assert.IsType<CompositeDecoratedDecoratorA>(compositeComposite.Decorated);
            var baseImpl = compositeComposite.Decorated.Decorated;
            Assert.True(baseImpl.GetType() == typeof(CompositeDecoratedBasisA) || baseImpl.GetType() == typeof(CompositeDecoratedBasisB));
        }
        Assert.Equal(2, composites.Count);
    }
}