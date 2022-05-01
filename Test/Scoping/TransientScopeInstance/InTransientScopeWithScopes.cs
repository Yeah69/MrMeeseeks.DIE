using System;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Scoping.TransientScopeInstance.InContainer.InTransientScopeWithScopes;

internal interface IInterface
{
    bool IsDisposed { get; }
}

internal class Dependency : IInterface, ITransientScopeInstance, IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = false;
}

internal interface ITransientScopeChild
{
    IInterface TransientScopeInstance { get; }
    bool Disposed { get; }
}

internal class TransientScopeChild : ITransientScopeChild, IScopeRoot, IDisposable
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

internal class TransientScopeWithScopes : ITransientScopeRoot
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
internal partial class Container
{
    
}

public partial class Tests
{
    [Fact]
    public void TransientScopeWithScopes()
    {
        using var container = new Container();
        var transientScopeRoot = container.Create();
        Assert.NotEqual(transientScopeRoot.A, transientScopeRoot.B);
        Assert.Equal(transientScopeRoot.A.TransientScopeInstance, transientScopeRoot.B.TransientScopeInstance);
        transientScopeRoot.CleanUp();
        Assert.True(transientScopeRoot.A.Disposed);
        Assert.True(transientScopeRoot.B.Disposed);
        container.Dispose();
    }
}