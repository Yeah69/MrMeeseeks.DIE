using System;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class Dependency
{
    internal Dependency(Lazy<Root> _)
    {
    }
}

internal class Root
{
    internal Root(Dependency _) {}
}

[CreateFunction(typeof(Root), "Create")]
internal partial class Container
{
}