﻿using System;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class Root
{
    internal Root((Dependency, Dependency) inner, Func<Dependency> two) {}
}

internal class Dependency
{
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    private Container() {}
}