using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Configuration.SyncTransient;

internal class Managed : IDisposable
{
    public void Dispose()
    {
    }
}

// ReSharper disable once UnusedTypeParameter
internal class Class<T0> : ISyncTransient, IDisposable
{
    // ReSharper disable once UnusedParameter.Local
    internal Class(Managed _) { }
    internal bool IsDisposed { get; private set; }
    public void Dispose() => IsDisposed = true;
}

[CreateFunction(typeof(Class<int>), "Create")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        using var instance = container.Create();
        Assert.False(instance.IsDisposed);
        container.Dispose();
        Assert.False(instance.IsDisposed);
    }
}