using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.ErrorCase.Sync.AfterContainerInstance;

internal sealed class DisposableDependency : IDisposable
{
    internal required DisposalTracking DisposalTracking { get; init; }
    public void Dispose() => DisposalTracking.RegisterDisposal(this);
}

/// <summary>
/// Makes the container mixed: sync and async disposal.
/// </summary>
internal sealed class DummyDependencyAsync : IAsyncDisposable
{
    public async ValueTask DisposeAsync() => await Task.Yield();
}

internal sealed class ThrowingDependency
{
    internal ThrowingDependency() => throw new Exception("Yikes!");
}

internal sealed class ContainerInstance : IContainerInstance
{
    internal ContainerInstance(DisposableDependency _, DummyDependencyAsync __) { }
}

internal sealed class Parent
{
    internal Parent(ContainerInstance _, ThrowingDependency __) {}
}

internal sealed class DisposalTracking : IContainerInstance
{
    internal List<object> DisposedObjects { get; } = [];
    
    internal void RegisterDisposal(object disposedObject) => DisposedObjects.Add(disposedObject);
}

[CreateFunction(typeof(Parent), "Create")]
[CreateFunction(typeof(DisposalTracking), "CreateDisposalTracking")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        try
        {
            _ = container.Create();
        }
        catch (SyncDisposalTriggeredException e)
        {
            await e.AsyncDisposal;
            Assert.Equal("Yikes!", e.Exception?.Message);
            Assert.True(container.CreateDisposalTracking().DisposedObjects is []);
            return;
        }
        Assert.Fail();
    }
}