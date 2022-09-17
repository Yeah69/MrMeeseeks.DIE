using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.Async.Awaited.TransientScopeInstanceFunction_DifferentSynchronicity;

internal interface IInterface {}

internal class DependencyA : IInterface, ITaskInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class DependencyB : IInterface
{
}

internal class Instance : ITransientScopeInstance
{
    public IInterface Inner { get; }

    public Instance(IInterface inner)
    {
        Inner = inner;
    }
}

internal class TransientScopeRoot0 : ITransientScopeRoot
{
    public Instance Dependency { get; }

    internal TransientScopeRoot0(Instance dependency)
    {
        Dependency = dependency;
    }
}

internal class TransientScopeRoot1 : ITransientScopeRoot
{
    internal TransientScopeRoot1(Instance dependency)
    {
        
    }
}

[FilterImplementationAggregation(typeof(DependencyB))]
[CreateFunction(typeof(TransientScopeRoot0), "Create0")]
[CreateFunction(typeof(TransientScopeRoot1), "Create1")]
internal sealed partial class Container
{
    [CustomScopeForRootTypes(typeof(TransientScopeRoot0))]
    private sealed partial class DIE_TransientScope0
    {
        
    }
    
    [FilterImplementationAggregation(typeof(DependencyA))]
    [ImplementationAggregation(typeof(DependencyB))]
    [CustomScopeForRootTypes(typeof(TransientScopeRoot1))]
    private sealed partial class DIE_TransientScope1
    {
        
    }
}