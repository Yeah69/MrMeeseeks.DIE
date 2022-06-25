using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.Async.InScope;

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

[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container
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