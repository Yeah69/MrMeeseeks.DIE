using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.SyncTransient;

internal class Managed : IDisposable
{
    public void Dispose()
    {
    }
}

internal class Class<T0> : ISyncTransient, IDisposable
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
    public async ValueTask Test()
    {
        await using var container = new Container();
        using var instance = container.Create();
        Assert.False(instance.IsDisposed);
        container.Dispose();
        Assert.False(instance.IsDisposed);
    }
}