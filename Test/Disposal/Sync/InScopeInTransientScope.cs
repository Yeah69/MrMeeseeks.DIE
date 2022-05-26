using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.Sync.InScopeInTransientScope;

internal class Dependency :  IDisposable
{
    public bool IsDisposed { get; private set; }
    
    public void Dispose() => IsDisposed = true;
}

internal class ScopeRoot : IScopeRoot
{
    public ScopeRoot(Dependency dependency) => Dependency = dependency;

    internal Dependency Dependency { get; }
}

internal class TransientScopeRoot : ITransientScopeRoot
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
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var transientScopeRoot = container.Create();
        Assert.False(transientScopeRoot.ScopeRoot.Dependency.IsDisposed);
        transientScopeRoot.Cleanup();
        Assert.True(transientScopeRoot.ScopeRoot.Dependency.IsDisposed);
    }
}