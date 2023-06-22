using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal struct Dependency {}

internal class Root
{
    internal Root(Dependency? dependency) => Dependency = dependency;
    
    internal Dependency? Dependency { get; }
}

[FilterImplementationAggregation(typeof(Dependency))]
[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    private Container() {}
}