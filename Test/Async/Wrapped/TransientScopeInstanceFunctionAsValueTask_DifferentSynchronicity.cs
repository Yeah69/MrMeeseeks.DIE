using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Async.Wrapped.TransientScopeInstanceFunctionAsValueTask_DifferentSynchronicity;

internal interface IInterface {}

internal class DependencyA : IInterface, ITaskInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskInitializer.InitializeAsync()
    {
        await Task.Delay(500);
        IsInitialized = true;
    }
}

internal class DependencyB : IInterface { }

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
    public ValueTask<Instance> Dependency { get; }

    internal TransientScopeRoot0(ValueTask<Instance> dependency)
    {
        Dependency = dependency;
    }
}

internal class TransientScopeRoot1 : ITransientScopeRoot
{
    // ReSharper disable once UnusedParameter.Local
    internal TransientScopeRoot1(ValueTask<Instance> dependency) { }
}

[FilterImplementationAggregation(typeof(DependencyB))]
[CreateFunction(typeof(TransientScopeRoot0), "Create0")]
[CreateFunction(typeof(TransientScopeRoot1), "Create1")]
internal sealed partial class Container
{
    [CustomScopeForRootTypes(typeof(TransientScopeRoot0))]
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_TransientScope0 { }
    
    [FilterImplementationAggregation(typeof(DependencyA))]
    [ImplementationAggregation(typeof(DependencyB))]
    [CustomScopeForRootTypes(typeof(TransientScopeRoot1))]
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_TransientScope1 { }
}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance0 = container.Create0();
        var _ = container.Create1();
        Assert.True(((await instance0.Dependency).Inner as DependencyA)?.IsInitialized);
    }
}