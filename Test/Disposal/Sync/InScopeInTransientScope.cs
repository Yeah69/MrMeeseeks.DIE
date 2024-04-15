using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.Sync.InScopeInTransientScope;

internal sealed class Dependency :  IDisposable
{
    public bool IsDisposed { get; private set; }
    
    public void Dispose() => IsDisposed = true;
}

internal sealed class ScopeRoot : IScopeRoot
{
    public ScopeRoot(Dependency dependency) => Dependency = dependency;

    internal Dependency Dependency { get; }
}

internal sealed class TransientScopeRoot : ITransientScopeRoot
{
    public ScopeRoot ScopeRoot { get; }
    private readonly IDisposable _disposable;

    internal TransientScopeRoot(
        ScopeRoot scopeRoot,
        IDisposable disposable)
    {
        ScopeRoot = scopeRoot;
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
        var dependency = container.Create();
        try
        {
            Assert.False(dependency.ScopeRoot.Dependency.IsDisposed);
            container.Dispose();
        }
        catch (SyncDisposalTriggeredException e)
        {
            await e.AsyncDisposal;
            Assert.True(dependency.ScopeRoot.Dependency.IsDisposed);
            return;
        }
        Assert.Fail();
    }
}