using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.TransientScopeDisposalHandle.SyncContainerToSyncDisposalHandle;

internal sealed class Dependency : IDisposable
{
    internal bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

internal sealed class TransientScopeRoot : ITransientScopeRoot
{
    internal Dependency Dependency { get; }
    private readonly IDisposable _disposable;

    internal TransientScopeRoot(
        Dependency dependency,
        IDisposable disposable)
    {
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
        await using var container = Container.DIE_CreateContainer();
        var transientScopeRoot = container.Create();
        try
        {
            Assert.False(transientScopeRoot.Dependency.IsDisposed);
            transientScopeRoot.Cleanup();
        }
        catch (SyncDisposalTriggeredException e)
        {
            await e.AsyncDisposal;
            Assert.True(transientScopeRoot.Dependency.IsDisposed);
            return;
        }
        Assert.Fail();
    }
}