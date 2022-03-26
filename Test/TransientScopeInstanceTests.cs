using System;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test;

internal interface ITransientScopeInstanceInner {}
internal class TransientScopeInstance : ITransientScopeInstanceInner, ITransientScopeInstance {}

public partial class TransientScopeInstanceTests
{
    [Fact]
    public void InContainer()
    {
        using var container = new TransientScopeInstanceContainer();
        var _ = container.Create0();
    }
}

internal interface IScopeWithTransientScopeInstance {}

internal class ScopeWithTransientScopeInstance : IScopeWithTransientScopeInstance, IScopeRoot
{
    public ScopeWithTransientScopeInstance(ITransientScopeInstanceInner _) {}
}

public partial class TransientScopeInstanceTests
{
    [Fact]
    public void InScope()
    {
        using var container = new TransientScopeInstanceContainer();
        var _ = container.Create1();
    }
}

internal interface IScopeWithTransientScopeInstanceAbove {}

internal class ScopeWithTransientScopeInstanceAbove : IScopeWithTransientScopeInstanceAbove, IScopeRoot
{
    public ScopeWithTransientScopeInstanceAbove(IScopeWithTransientScopeInstance _) {}
}

public partial class TransientScopeInstanceTests
{
    [Fact]
    public void InScopeInScope()
    {
        using var container = new TransientScopeInstanceContainer();
        var _ = container.Create2();
    }
}

internal interface ITransientScopeWithTransientScopeInstance {}

internal class TransientScopeWithTransientScopeInstance : ITransientScopeWithTransientScopeInstance, ITransientScopeRoot
{
    public TransientScopeWithTransientScopeInstance(IDisposable scopeDisposal, ITransientScopeInstanceInner _) {}
}

public partial class TransientScopeInstanceTests
{
    [Fact]
    public void InTransientScope()
    {
        using var container = new TransientScopeInstanceContainer();
        var _ = container.Create3();
    }
}

internal interface ITransientScopeChild
{
    ITransientScopeInstanceInner TransientScopeInstance { get; }
    bool Disposed { get; }
}

internal class TransientScopeChild : ITransientScopeChild, IScopeRoot, IDisposable
{
    public TransientScopeChild(ITransientScopeInstanceInner transientScopeInstance)
    {
        TransientScopeInstance = transientScopeInstance;
    }

    public ITransientScopeInstanceInner TransientScopeInstance { get; }
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        Disposed = true;
    }
}

internal interface ITransientScopeWithScopes
{
    ITransientScopeChild A { get; }
    ITransientScopeChild B { get; }
    void CleanUp();
}

internal class TransientScopeWithScopes : ITransientScopeWithScopes, ITransientScopeRoot
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

[CreateFunction(typeof(ITransientScopeInstanceInner), "Create0")]
[CreateFunction(typeof(IScopeWithTransientScopeInstance), "Create1")]
[CreateFunction(typeof(IScopeWithTransientScopeInstanceAbove), "Create2")]
[CreateFunction(typeof(ITransientScopeWithTransientScopeInstance), "Create3")]
[CreateFunction(typeof(ITransientScopeWithScopes), "Create4")]
internal partial class TransientScopeInstanceContainer
{
    
}

public partial class TransientScopeInstanceTests
{
    [Fact]
    public void TransientScopeWithScopes()
    {
        using var container = new TransientScopeInstanceContainer();
        var transientScopeRoot = container.Create4();
        Assert.NotEqual(transientScopeRoot.A, transientScopeRoot.B);
        Assert.Equal(transientScopeRoot.A.TransientScopeInstance, transientScopeRoot.B.TransientScopeInstance);
        transientScopeRoot.CleanUp();
        Assert.True(transientScopeRoot.A.Disposed);
        Assert.True(transientScopeRoot.B.Disposed);
    }
}