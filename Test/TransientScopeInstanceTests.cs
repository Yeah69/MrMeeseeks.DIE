using System;
using Xunit;

namespace MrMeeseeks.DIE.Test;

internal interface ITransientScopeInstanceInner {}
internal class TransientScopeInstance : ITransientScopeInstanceInner, ITransientScopeInstance {}

internal partial class TransientScopeInstanceContainer : IContainer<ITransientScopeInstanceInner>
{
    
}

public partial class TransientScopeInstanceTests
{
    [Fact]
    public void InContainer()
    {
        using var container = new TransientScopeInstanceContainer();
        var _ = ((IContainer<ITransientScopeInstanceInner>) container).Resolve();
    }
}

internal interface IScopeWithTransientScopeInstance {}

internal class ScopeWithTransientScopeInstance : IScopeWithTransientScopeInstance, IScopeRoot
{
    public ScopeWithTransientScopeInstance(ITransientScopeInstanceInner _) {}
}

internal partial class TransientScopeInstanceContainer : IContainer<IScopeWithTransientScopeInstance>
{
    
}

public partial class TransientScopeInstanceTests
{
    [Fact]
    public void InScope()
    {
        using var container = new TransientScopeInstanceContainer();
        var _ = ((IContainer<IScopeWithTransientScopeInstance>) container).Resolve();
    }
}

internal interface IScopeWithTransientScopeInstanceAbove {}

internal class ScopeWithTransientScopeInstanceAbove : IScopeWithTransientScopeInstanceAbove, IScopeRoot
{
    public ScopeWithTransientScopeInstanceAbove(IScopeWithTransientScopeInstance _) {}
}

internal partial class TransientScopeInstanceContainer : IContainer<IScopeWithTransientScopeInstanceAbove>
{
    
}

public partial class TransientScopeInstanceTests
{
    [Fact]
    public void InScopeInScope()
    {
        using var container = new TransientScopeInstanceContainer();
        var _ = ((IContainer<IScopeWithTransientScopeInstanceAbove>) container).Resolve();
    }
}

internal interface ITransientScopeWithTransientScopeInstance {}

internal class TransientScopeWithTransientScopeInstance : ITransientScopeWithTransientScopeInstance, ITransientScopeRoot
{
    public TransientScopeWithTransientScopeInstance(IDisposable scopeDisposal, ITransientScopeInstanceInner _) {}
}

internal partial class TransientScopeInstanceContainer : IContainer<ITransientScopeWithTransientScopeInstance>
{
    
}

public partial class TransientScopeInstanceTests
{
    [Fact]
    public void InTransientScope()
    {
        using var container = new TransientScopeInstanceContainer();
        var _ = ((IContainer<ITransientScopeWithTransientScopeInstance>) container).Resolve();
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

internal partial class TransientScopeInstanceContainer : IContainer<ITransientScopeWithScopes>
{
    
}

public partial class TransientScopeInstanceTests
{
    [Fact]
    public void TransientScopeWithScopes()
    {
        using var container = new TransientScopeInstanceContainer();
        var transientScopeRoot = ((IContainer<ITransientScopeWithScopes>) container).Resolve();
        Assert.NotEqual(transientScopeRoot.A, transientScopeRoot.B);
        Assert.Equal(transientScopeRoot.A.TransientScopeInstance, transientScopeRoot.B.TransientScopeInstance);
        transientScopeRoot.CleanUp();
        Assert.True(transientScopeRoot.A.Disposed);
        Assert.True(transientScopeRoot.B.Disposed);
    }
}