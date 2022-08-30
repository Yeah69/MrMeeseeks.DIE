﻿using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.Func.AsyncWrapped.SingleValueTask;

internal class Dependency : IValueTaskTypeInitializer
{
    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

internal class OuterDependency
{
    internal OuterDependency(
        Dependency dependency)
    {
        
    }
}

[CreateFunction(typeof(Func<ValueTask<OuterDependency>>), "Create")]
internal sealed partial class Container
{
}