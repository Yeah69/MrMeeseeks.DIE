using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.Sync.InTransientScopeInTransientScope;

internal sealed class Dependency :  IDisposable
{
    public bool IsDisposed { get; private set; }
    
    public void Dispose() => IsDisposed = true;
}

internal sealed class TransientScopeRootInner : ITransientScopeRoot
{
    public TransientScopeRootInner(Dependency dependency) => Dependency = dependency;

    internal Dependency Dependency { get; }
}

internal sealed class TransientScopeRoot : ITransientScopeRoot
{
    public TransientScopeRootInner TransientScopeRootInner { get; }
    public Dependency Dependency { get; }
    private readonly IDisposable _disposable;

    internal TransientScopeRoot(
        TransientScopeRootInner transientScopeRootInner,
        Dependency dependency,
        IDisposable disposable)
    {
        TransientScopeRootInner = transientScopeRootInner;
        Dependency = dependency;
        _disposable = disposable;
    }

    internal void Cleanup() => _disposable.Dispose();
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = Container.DIE_CreateContainer();
        var transientScopeRoot = container.Create();
        var uncaught = true;
        try
        {
            Assert.False(transientScopeRoot.TransientScopeRootInner.Dependency.IsDisposed);
            Assert.False(transientScopeRoot.Dependency.IsDisposed);
            transientScopeRoot.Cleanup();
        }
        catch (SyncDisposalTriggeredException e)
        {
            await e.AsyncDisposal;
            Assert.False(transientScopeRoot.TransientScopeRootInner.Dependency.IsDisposed);
            Assert.True(transientScopeRoot.Dependency.IsDisposed);
            uncaught = false;
        }
        Assert.False(uncaught);
        
        try
        {
            container.Dispose();
        }
        catch (SyncDisposalTriggeredException e)
        {
            await e.AsyncDisposal;
            Assert.True(transientScopeRoot.TransientScopeRootInner.Dependency.IsDisposed);
            Assert.True(transientScopeRoot.Dependency.IsDisposed);
            return;
        }
        Assert.Fail();
    }
}