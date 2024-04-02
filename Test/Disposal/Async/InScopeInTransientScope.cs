using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.Async.InScopeInTransientScope;

internal sealed class Dependency :  IAsyncDisposable
{
    public bool IsDisposed { get; private set; }
    
    public async ValueTask DisposeAsync()
    {
        await Task.Delay(500);
        IsDisposed = true;
    }
}

internal sealed class ScopeRoot : IScopeRoot
{
    public ScopeRoot(Dependency dependency) => Dependency = dependency;

    internal Dependency Dependency { get; }
}

internal sealed class TransientScopeRoot : ITransientScopeRoot
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
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var transientScopeRoot = container.Create();
        Assert.False(transientScopeRoot.ScopeRoot.Dependency.IsDisposed);
        await transientScopeRoot.Cleanup();
        Assert.True(transientScopeRoot.ScopeRoot.Dependency.IsDisposed);
    }
}