using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class Root
{
    internal Root(Dependency inner, Dependency two) {}
}

internal class Dependency : IContainerInstance
{
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    private Container() {}
}