using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
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
    private Container() {}
    
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

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance0 = container.Create0();
        var _ = container.Create1();
        Assert.True((((await instance0.ConfigureAwait(false)).Dependency).Inner as DependencyA)?.IsInitialized);
    }
}