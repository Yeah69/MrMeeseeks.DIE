using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.ErrorCase.SyncOnly.Vanilla;

internal sealed class Dependency : IDisposable
{
    internal required DisposalTracking DisposalTracking { get; init; }
    public void Dispose() => DisposalTracking.RegisterDisposal(this);
}

internal sealed class Parent
{
    internal Parent(Dependency _) => throw new Exception("Yikes!");
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
        catch (AggregateException e)
        {
            Assert.True(e.InnerExceptions is [{Message: "Yikes!"}]);
            Assert.True(container.CreateDisposalTracking().DisposedObjects is [Dependency]);
            return;
        }
        Assert.Fail();
    }
}