using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.Async.InTransientScopeInTransientScope;

internal class Dependency :  IAsyncDisposable
{
    public bool IsDisposed { get; private set; }
    
    public async ValueTask DisposeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsDisposed = true;
    }
}

internal class TransientScopeRootInner : ITransientScopeRoot
{
    public TransientScopeRootInner(Dependency dependency) => Dependency = dependency;

    internal Dependency Dependency { get; }
}

internal class TransientScopeRoot : ITransientScopeRoot
{
    public TransientScopeRootInner TransientScopeRootInner { get; }
    public Dependency Dependency { get; }
    private readonly IAsyncDisposable _disposable;

    internal TransientScopeRoot(
        TransientScopeRootInner transientScopeRootInner,
        Dependency dependency,
        IAsyncDisposable disposable)
    {
        TransientScopeRootInner = transientScopeRootInner;
        Dependency = dependency;
        _disposable = disposable;
    }

    internal ValueTask Cleanup() => _disposable.DisposeAsync();
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var transientScopeRoot = container.Create();
        Assert.False(transientScopeRoot.TransientScopeRootInner.Dependency.IsDisposed);
        Assert.False(transientScopeRoot.Dependency.IsDisposed);
        await transientScopeRoot.Cleanup().ConfigureAwait(false);
        Assert.False(transientScopeRoot.TransientScopeRootInner.Dependency.IsDisposed);
        Assert.True(transientScopeRoot.Dependency.IsDisposed);
        await container.DisposeAsync().ConfigureAwait(false);
        Assert.True(transientScopeRoot.TransientScopeRootInner.Dependency.IsDisposed);
        Assert.True(transientScopeRoot.Dependency.IsDisposed);
    }
}