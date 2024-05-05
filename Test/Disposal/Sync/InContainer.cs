using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.Sync.InContainer;

internal sealed class Dependency :  IDisposable
{
    public bool IsDisposed { get; private set; }
    
    public void Dispose()
    {
        IsDisposed = true;
    }
}

/// <summary>
/// Makes the container mixed: sync and async disposal.
/// </summary>
internal sealed class DummyDependencyAsync : IAsyncDisposable
{
    public ValueTask DisposeAsync() => new(Task.CompletedTask);
}

[CreateFunction(typeof(DummyDependencyAsync), "DummyCreate")]
[CreateFunction(typeof(Dependency), "Create")]
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
            Assert.False(dependency.IsDisposed);
            container.Dispose();
        }
        catch (SyncDisposalTriggeredException e)
        {
            await e.AsyncDisposal;
            Assert.True(dependency.IsDisposed);
            return;
        }
        Assert.Fail();
    }
}