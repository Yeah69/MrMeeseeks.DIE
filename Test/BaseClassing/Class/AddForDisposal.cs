using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.BaseClassing.Class.AddForDisposal;

internal class Dependency : IDisposable
{
    internal bool IsDisposed { get; private set; }
    public void Dispose() => IsDisposed = true;
}

[CreateFunction(typeof(Dependency), "Create")]
internal abstract class ContainerBase
{
    protected abstract void DIE_AddForDisposal(IDisposable disposable);
    
    protected Dependency DIE_Factory_Dependency()
    {
        var dependency = new Dependency();
        DIE_AddForDisposal(dependency);
        return dependency;
    }
}

internal sealed partial class Container : ContainerBase
{
    private Container() {}
    protected override partial void DIE_AddForDisposal(IDisposable disposable);
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        container.Dispose();
        Assert.True(instance.IsDisposed);
    }
}