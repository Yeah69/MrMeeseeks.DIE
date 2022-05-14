using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.Async.InTransientScope;

internal class Dependency :  IAsyncDisposable
{
    public bool IsDisposed { get; private set; }
    
    public async ValueTask DisposeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsDisposed = true;
    }
}

internal class TransientScopeRoot : ITransientScopeRoot
{
    public TransientScopeRoot(Dependency dependency) => Dependency = dependency;

    internal Dependency Dependency { get; }
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
        var dependency = container.Create();
        Assert.False(dependency.Dependency.IsDisposed);
        await container.DisposeAsync().ConfigureAwait(false);
        Assert.True(dependency.Dependency.IsDisposed);
    }
}