using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
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

internal class TransientScopeChild(IInterface transientScopeInstance) : ITransientScopeChild, IScopeRoot, IDisposable
{
    public IInterface TransientScopeInstance { get; } = transientScopeInstance;
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        Disposed = true;
    }
}

internal class TransientScopeWithScopes(IDisposable scopeDisposal, ITransientScopeChild a, ITransientScopeChild b)
    : ITransientScopeRoot
{
    public ITransientScopeChild A { get; } = a;
    public ITransientScopeChild B { get; } = b;

    public void CleanUp()
    {
        scopeDisposal.Dispose();
    }
}

[CreateFunction(typeof(TransientScopeWithScopes), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var transientScopeRoot = container.Create();
        Assert.NotEqual(transientScopeRoot.A, transientScopeRoot.B);
        Assert.Equal(transientScopeRoot.A.TransientScopeInstance, transientScopeRoot.B.TransientScopeInstance);
        transientScopeRoot.CleanUp();
        Assert.True(transientScopeRoot.A.Disposed);
        Assert.True(transientScopeRoot.B.Disposed);
        container.Dispose();
    }
}