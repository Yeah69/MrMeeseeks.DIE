using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.TransientScopeDisposalHandle.AsyncContainerToAsyncDisposalHandle;

internal class Dependency : IAsyncDisposable
{
    internal bool IsDisposed { get; private set; }

    public async ValueTask DisposeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsDisposed = true;
    }
}

internal class TransientScopeRoot : ITransientScopeRoot
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
internal partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var transientScopeRoot = container.Create();
        Assert.False(transientScopeRoot.Dependency.IsDisposed);
        await transientScopeRoot.Cleanup().ConfigureAwait(false);
        Assert.True(transientScopeRoot.Dependency.IsDisposed);
    }
}