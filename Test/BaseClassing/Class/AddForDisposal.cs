﻿using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.BaseClassing.Class.AddForDisposal;

internal sealed class Dependency : IDisposable
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
    protected override partial void DIE_AddForDisposal(IDisposable disposable);
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        await container.DisposeAsync();
        Assert.True(instance.IsDisposed);
    }
}