using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Configuration.SyncTransientWithJustTransient;

internal sealed class Managed : IDisposable
{
    public void Dispose()
    {
    }
}

// ReSharper disable once UnusedTypeParameter
internal sealed class Class<T0> : ITransient, IDisposable
{
    // ReSharper disable once UnusedParameter.Local
    internal Class(Managed _) { }
    internal bool IsDisposed { get; private set; }
    public void Dispose() => IsDisposed = true;
}

[CreateFunction(typeof(Class<int>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        using var instance = container.Create();
        try
        {
            Assert.False(instance.IsDisposed);
            container.Dispose();
        }
        catch (SyncDisposalTriggeredException e)
        {
            await e.AsyncDisposal;
            Assert.False(instance.IsDisposed);
            return;
        }
        Assert.Fail();
    }
}