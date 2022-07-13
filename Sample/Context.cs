﻿using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.Func.OverrideScoped;

internal class DependencyInner
{
    internal DependencyInner(
        Lazy<Parent> lazyParent,
        string asdf)
    {
        
    }
}

internal class Dependency
{
    public int Value { get; }

    internal Dependency(int value, Func<string, DependencyInner> fac) => Value = value;
}

internal class Parent : IScopeRoot
{
    public Dependency Dependency { get; }
    
    internal Parent(
        Func<int, Dependency> fac) =>
        Dependency = fac(1);
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container
{
    private int DIE_Factory_int => 0;
}