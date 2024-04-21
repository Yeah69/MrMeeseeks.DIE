using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.GenericDisposableMixedWithNonGeneric;

// ReSharper disable once UnusedTypeParameter
internal sealed class Dependency<T> : IDisposable
{
    internal bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

internal sealed class Dependency : IDisposable
{
    internal bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

[CreateFunction(typeof((Dependency<int>, Dependency)), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        var container = Container.DIE_CreateContainer();
        var (generic, nonGeneric) = container.Create();
        await container.DisposeAsync();
        Assert.True(generic.IsDisposed);
        Assert.True(nonGeneric.IsDisposed);
    }
}