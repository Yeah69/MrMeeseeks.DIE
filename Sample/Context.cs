﻿using System;
using System.IO;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test.ConstructorChoice.WithParameter;

internal class Dependency : IContainerInstance {}

internal class Parent
{
    internal Parent(
        Dependency dependency0,
        Dependency dependency1,
        Dependency dependency2,
        Dependency dependency3,
        Dependency dependency4,
        Dependency dependency5,
        Dependency dependency6,
        Dependency dependency7,
        Dependency dependency8,
        Dependency dependency9){}
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container
{
    
}