using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.Async.InScopeInTransientScope;

internal class Dependency :  IAsyncDisposable
{
    public bool IsDisposed { get; private set; }
    
    public async ValueTask DisposeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsDisposed = true;
    }
}

internal class ScopeRoot : IScopeRoot
{
    public ScopeRoot(Dependency dependency) => Dependency = dependency;

    internal Dependency Dependency { get; }
}

internal class TransientScopeRoot : ITransientScopeRoot
{
    public ScopeRoot ScopeRoot { get; }
    private readonly IAsyncDisposable _disposable;

    internal TransientScopeRoot(
        ScopeRoot scopeRoot,
        IAsyncDisposable disposable)
    {
        ScopeRoot = scopeRoot;
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
        Assert.False(transientScopeRoot.ScopeRoot.Dependency.IsDisposed);
        await transientScopeRoot.Cleanup().ConfigureAwait(false);
        Assert.True(transientScopeRoot.ScopeRoot.Dependency.IsDisposed);
    }
}