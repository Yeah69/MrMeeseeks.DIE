using System;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.SyncTransientWithJustTransient;

internal class Managed : IDisposable
{
    public void Dispose()
    {
    }
}

internal class Class<T0> : ITransient, IDisposable
{
    internal Class(Managed _) { }
    internal bool IsDisposed { get; private set; }
    public void Dispose() => IsDisposed = true;
}

[CreateFunction(typeof(Class<int>), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        using var instance = container.Create();
        Assert.False(instance.IsDisposed);
        container.Dispose();
        Assert.False(instance.IsDisposed);
    }
}