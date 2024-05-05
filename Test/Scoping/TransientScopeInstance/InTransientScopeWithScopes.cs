using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.TransientScopeInstance.InContainer.InTransientScopeWithScopes;

internal interface IInterface
{
    bool IsDisposed { get; }
}

internal sealed class Dependency : IInterface, ITransientScopeInstance, IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = false;
}

internal interface ITransientScopeChild
{
    IInterface TransientScopeInstance { get; }
    bool Disposed { get; }
}

internal sealed class TransientScopeChild : ITransientScopeChild, IScopeRoot, IDisposable
{
    public TransientScopeChild(IInterface transientScopeInstance)
    {
        TransientScopeInstance = transientScopeInstance;
    }

    public IInterface TransientScopeInstance { get; }
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        Disposed = true;
    }
}

internal sealed class TransientScopeWithScopes : ITransientScopeRoot
{
    private readonly IDisposable _scopeDisposal;

    public TransientScopeWithScopes(IDisposable scopeDisposal, ITransientScopeChild a, ITransientScopeChild b)
    {
        _scopeDisposal = scopeDisposal;
        A = a;
        B = b;
    }
    public ITransientScopeChild A { get; }
    public ITransientScopeChild B { get; }
    public void CleanUp()
    {
        _scopeDisposal.Dispose();
    }
}

[CreateFunction(typeof(TransientScopeWithScopes), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var transientScopeRoot = container.Create();
        Assert.NotEqual(transientScopeRoot.A, transientScopeRoot.B);
        Assert.Equal(transientScopeRoot.A.TransientScopeInstance, transientScopeRoot.B.TransientScopeInstance);
        transientScopeRoot.CleanUp();
        Assert.True(transientScopeRoot.A.Disposed);
        Assert.True(transientScopeRoot.B.Disposed);
    }
}