using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.TransientScopeDisposalHandle.NoneContainerToAsyncDisposalHandle;

internal class Dependency
{
    internal bool IsDisposed => true;
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
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public async Task Test()
    {
        var container = new Container();
        var transientScopeRoot = container.Create();
        Assert.True(transientScopeRoot.Dependency.IsDisposed);
        await transientScopeRoot.Cleanup().ConfigureAwait(false);
        Assert.True(transientScopeRoot.Dependency.IsDisposed);
    }
}