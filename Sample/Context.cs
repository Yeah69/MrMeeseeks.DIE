using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.CycleDetection.Function.NoCycle.DirectRecursionScope;

internal class Dependency : IScopeInstance
{
    internal Dependency(Func<Dependency> inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal partial class Container
{
    
}