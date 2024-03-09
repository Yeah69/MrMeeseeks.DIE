using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency : IScopeInstance
{
    public int ValueInt { get; }

    internal Dependency(
        int valueInt)
    {
        ValueInt = valueInt;
    }
}

internal class Parent0 : IScopeRoot
{
    public Dependency Dependency { get; }
    
    internal Parent0(
        Dependency dependency) =>
        Dependency = dependency;
}

internal class Parent1
{
    public Dependency Dependency { get; }
    
    internal Parent1(
        Func<int, Parent0> fac) =>
        Dependency = fac(2).Dependency;
}

[CreateFunction(typeof(Func<int, Parent1>), "Create")]
internal sealed partial class Container;