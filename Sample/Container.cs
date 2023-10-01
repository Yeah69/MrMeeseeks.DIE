using System;
using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

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