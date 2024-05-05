using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.SyncOnly.InContainer;

internal sealed class Dependency :  IDisposable
{
    public bool IsDisposed { get; private set; }
    
    public void Dispose()
    {
        IsDisposed = true;
    }
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        using var container = Container.DIE_CreateContainer();
        var dependency = container.Create();
        Assert.False(dependency.IsDisposed);
        container.Dispose();
        Assert.True(dependency.IsDisposed);
    }
}