using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Disposal.GenericDisposable;

// ReSharper disable once UnusedTypeParameter
internal sealed class Dependency<T> : IDisposable
{
    internal bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

[CreateFunction(typeof(Dependency<>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        var container = Container.DIE_CreateContainer();
        var instance = container.Create<int>();
        await container.DisposeAsync();
        Assert.True(instance.IsDisposed);
    }
}