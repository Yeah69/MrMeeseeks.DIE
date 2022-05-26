using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.AsyncTransient;

internal class Managed : IAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

internal class Class<T0> : IAsyncTransient, IAsyncDisposable
{
    internal Class(Managed _) { }
    internal bool IsDisposed { get; private set; }
    public async ValueTask DisposeAsync()
    {
        await Task.Delay(500);
        IsDisposed = true;
    }
}

[CreateFunction(typeof(Class<int>), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        await using var instance = container.Create();
        Assert.False(instance.IsDisposed);
        await container.DisposeAsync().ConfigureAwait(false);
        Assert.False(instance.IsDisposed);
    }
}