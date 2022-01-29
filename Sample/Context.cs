using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.ScopeSpecificAttributesTestsWithImplementations;

internal interface IDependency {}

internal class DependencyContainer : IDependency {}

internal class DependencyTransientScope : IDependency {}

internal class DependencyScope : IDependency {}

internal class TransientScope : ITransientScopeRoot
{
    public TransientScope(IReadOnlyList<IDependency> dependencies) => Dependencies = dependencies;

    public IReadOnlyList<IDependency> Dependencies { get; }
}

internal class Scope : IScopeRoot
{
    public Scope(IReadOnlyList<IDependency> dependencies) => Dependencies = dependencies;

    public IReadOnlyList<IDependency> Dependencies { get; }
}

internal partial class Container : IContainer<IReadOnlyList<IDependency>>, IContainer<TransientScope>, IContainer<Scope>
{
    [FilterImplementationAggregation(typeof(DependencyContainer))]
    [FilterImplementationAggregation(typeof(DependencyScope))]
    internal partial class DIE_DefaultTransientScope
    {
        
    }

    [FilterImplementationAggregation(typeof(DependencyContainer))]
    [FilterImplementationAggregation(typeof(DependencyTransientScope))]
    internal partial class DIE_DefaultScope
    {
        
    }
}