using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.TransientScopeDisposalHandle.AsyncContainerToAsyncDisposalHandle;

internal sealed class Dependency : IAsyncDisposable
{
    internal bool IsDisposed { get; private set; }

    public async ValueTask DisposeAsync()
    {
        await Task.Delay(500);
        IsDisposed = true;
    }
}

internal sealed class TransientScopeRoot : ITransientScopeRoot
{
    internal Dependency Dependency { get; }
    private readonly IAsyncDisposable _disposable;

    internal TransientScopeRoot(
        Dependency dependency,
        IAsyncDisposable disposable)
    {
        Dependency = dependency;
        _disposable = disposable;
    }

    internal ValueTask Cleanup() => _disposable.DisposeAsync();
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
        Assert.False(transientScopeRoot.Dependency.IsDisposed);
        await transientScopeRoot.Cleanup();
        Assert.True(transientScopeRoot.Dependency.IsDisposed);
    }
}