using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Disposal.Calls.Sync;

internal class Dependency :  IDisposable, IAsyncDisposable, IAsyncTransient
{
    public bool IsSyncDisposedCalled { get; private set; }
    
    public bool IsAsyncDisposedCalled { get; private set; }
    
    public void Dispose()
    {
        IsSyncDisposedCalled = true;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Yield();
        IsAsyncDisposedCalled = true;
    }
}

[CreateFunction(typeof(Dependency), "Create")]
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
        await container.DisposeAsync().ConfigureAwait(false);
        Assert.False(dependency.IsAsyncDisposedCalled);
        Assert.True(dependency.IsSyncDisposedCalled);
    }
}