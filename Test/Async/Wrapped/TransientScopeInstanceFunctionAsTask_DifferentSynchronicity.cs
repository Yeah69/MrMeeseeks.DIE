using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Async.Wrapped.TransientScopeInstanceFunctionAsTask_DifferentSynchronicity;

internal interface IInterface {}

internal class DependencyA : IInterface, ITaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
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
    public Task<Instance> Dependency { get; }

    internal TransientScopeRoot0(Task<Instance> dependency)
    {
        Dependency = dependency;
    }
}

internal class TransientScopeRoot1 : ITransientScopeRoot
{
    internal TransientScopeRoot1(Task<Instance> dependency)
    {
        
    }
}

[FilterImplementationAggregation(typeof(DependencyB))]
[CreateFunction(typeof(TransientScopeRoot0), "Create0")]
[CreateFunction(typeof(TransientScopeRoot1), "Create1")]
internal partial class Container
{
    [CustomScopeForRootTypes(typeof(TransientScopeRoot0))]
    private partial class DIE_TransientScope0
    {
        
    }
    
    [FilterImplementationAggregation(typeof(DependencyA))]
    [ImplementationAggregation(typeof(DependencyB))]
    [CustomScopeForRootTypes(typeof(TransientScopeRoot1))]
    private partial class DIE_TransientScope1
    {
        
    }
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        using var container = new Container();
        var instance0 = container.Create0();
        var _ = container.Create1();
        Assert.True(((await instance0.Dependency.ConfigureAwait(false)).Inner as DependencyA)?.IsInitialized);
    }
}