using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.Composite.ScopeRoot;

internal interface IInterface
{
    IReadOnlyList<IInterface> Composites { get; }
    IDependency Dependency { get; }
}

internal interface IDependency { }

internal class Dependency : IDependency, IScopeInstance { }

internal class BasisA : IInterface, IScopeRoot
{
    public BasisA(IDependency dependency) => Dependency = dependency;

    public IReadOnlyList<IInterface> Composites => new List<IInterface> { this };
    public IDependency Dependency { get; }
}

internal class BasisB : IInterface
{
    public BasisB(IDependency dependency) => Dependency = dependency;

    public IReadOnlyList<IInterface> Composites => new List<IInterface> { this };
    public IDependency Dependency { get; }
}

internal class Composite : IInterface, IComposite<IInterface>
{
    public Composite(IReadOnlyList<IInterface> composites, IDependency dependency)
    {
        Composites = composites;
        Dependency = dependency;
    }

    public IReadOnlyList<IInterface> Composites { get; }
    public IDependency Dependency { get; }
}

[CreateFunction(typeof(IInterface), "CreateDep")]
[CreateFunction(typeof(IReadOnlyList<IInterface>), "CreateCollection")]
internal sealed partial class Container
{
    
}