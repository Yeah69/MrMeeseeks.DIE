using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal interface IInterface {}

internal class Dependency : IInterface { }

internal class Root
{

    internal Root(Dependency? _)
    {
    }
}

[FilterImplementationAggregation(typeof(Dependency))]
[CreateFunction(typeof(Root), "Create")]
internal partial class Container
{
}