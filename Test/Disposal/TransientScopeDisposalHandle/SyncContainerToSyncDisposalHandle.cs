using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.TransientScopeDisposalHandle.SyncContainerToSyncDisposalHandle;

internal class Dependency : IDisposable
{
    internal bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

internal class TransientScopeRoot : ITransientScopeRoot
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
        Assert.False(transientScopeRoot.Dependency.IsDisposed);
        transientScopeRoot.Cleanup();
        Assert.True(transientScopeRoot.Dependency.IsDisposed);
    }
}