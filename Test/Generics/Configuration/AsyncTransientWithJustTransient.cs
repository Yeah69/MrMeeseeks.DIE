using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Configuration.AsyncTransientWithJustTransient;

internal sealed class Managed : IAsyncDisposable
{
    public ValueTask DisposeAsync() => default;
}

// ReSharper disable once UnusedTypeParameter
internal sealed class Class<T0> : ITransient, IAsyncDisposable
{
    // ReSharper disable once UnusedParameter.Local
    internal Class(Managed _) { }
    internal bool IsDisposed { get; private set; }
    public async ValueTask DisposeAsync()
    {
        await Task.Delay(500);
        IsDisposed = true;
    }
}

[CreateFunction(typeof(Class<int>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        await using var instance = container.Create();
        Assert.False(instance.IsDisposed);
        await container.DisposeAsync();
        Assert.False(instance.IsDisposed);
    }
}