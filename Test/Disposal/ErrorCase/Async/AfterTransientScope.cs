using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.ErrorCase.Async.AfterTransientScope;

internal sealed class DisposableDependency : IAsyncDisposable
{
    internal required DisposalTracking DisposalTracking { get; init; }
    public async ValueTask DisposeAsync()
    {
        await Task.Yield();
        DisposalTracking.RegisterDisposal(this);
    }
}

internal sealed class ThrowingDependency
{
    internal ThrowingDependency() => throw new Exception("Yikes!");
}

internal sealed class ScopeRoot : IScopeRoot
{
    internal ScopeRoot(DisposableDependency _) { }
}

internal sealed class Parent : IScopeRoot
{
    internal Parent(ScopeRoot _, ThrowingDependency __) {}
}

internal sealed class DisposalTracking : IContainerInstance
{
    internal List<object> DisposedObjects { get; } = [];
    
    internal void RegisterDisposal(object disposedObject) => DisposedObjects.Add(disposedObject);
}

[CreateFunction(typeof(Task<Parent>), "Create")]
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
            _ = await container.Create();
        }
        catch (SyncDisposalTriggeredException e)
        {
            await e.AsyncDisposal;
            Assert.Equal("Yikes!", e.Exception?.Message);
            Assert.True(container.CreateDisposalTracking().DisposedObjects is [DisposableDependency]);
            return;
        }
        Assert.Fail();
    }
}