﻿using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency : IAsyncDisposable
{
    internal bool IsDisposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return new ValueTask(Task.CompletedTask);
    }
}

internal class TransientScopeRoot : ITransientScopeRoot
{
    public Dependency Dependency { get; }

    internal TransientScopeRoot(Dependency dependency)
    {
        Dependency = dependency;
    }
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal sealed partial class Container
{
    
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultTransientScope
    {
        // ReSharper disable once InconsistentNaming
        private Dependency DIE_Factory_Dependency
        {
            get
            {
                var dependency = new Dependency();
                DIE_AddForDisposalAsync(dependency);
                return dependency;
            }
        }

        private partial void DIE_AddForDisposalAsync(IAsyncDisposable asyncDisposable);
    }
}
