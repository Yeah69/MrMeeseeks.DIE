﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.Async.Awaited.AsyncFunctionCallAsTask;


internal class Dependency : ITaskTypeInitializer
{
    public bool IsInitialized { get; private set; }
    
    async Task ITaskTypeInitializer.InitializeAsync()
    {
        await Task.Delay(500).ConfigureAwait(false);
        IsInitialized = true;
    }
}

internal class ScopeRoot : IScopeRoot
{
    public Dependency Dep { get; }
    internal ScopeRoot(Dependency dep)
    {
        Dep = dep;
    }
}

[CreateFunction(typeof(Task<ScopeRoot>), "Create")]
internal partial class Container
{
}