﻿using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.CycleDetection.Function.Cycle.IndirectRecursionContainer;

internal class Dependency : IContainerInstance
{
    internal Dependency(InnerDependency inner) {}
}

internal class InnerDependency : IContainerInstance
{
    internal InnerDependency(Dependency inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    
}