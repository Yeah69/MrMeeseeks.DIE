using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.TransientScopeInstance.InContainer.InTransientScope;

internal interface IInterface
{
    bool IsDisposed { get; }
}

internal sealed class Dependency : IInterface, ITransientScopeInstance, IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

internal sealed class TransientScope : ITransientScopeRoot
{
    private readonly IDisposable _scopeDisposal;
    public IInterface Dependency { get; }
    public TransientScope(IDisposable scopeDisposal, IInterface dependency)
    {
        _scopeDisposal = scopeDisposal;
        Dependency = dependency;
    }

    public void Cleanup() => _scopeDisposal.Dispose();
}

[CreateFunction(typeof(TransientScope), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var transientScopeRoot = container.Create();
        Assert.IsType<Dependency>(transientScopeRoot.Dependency);
        Assert.False(transientScopeRoot.Dependency.IsDisposed);
        transientScopeRoot.Cleanup();
        Assert.True(transientScopeRoot.Dependency.IsDisposed);
    }
}