using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.Calls.Both;

internal class Dependency :  IDisposable, IAsyncDisposable
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
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var dependency = container.Create();
        await container.DisposeAsync();
        Assert.True(dependency.IsAsyncDisposedCalled);
        Assert.True(dependency.IsSyncDisposedCalled);
    }
}