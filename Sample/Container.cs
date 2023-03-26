using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class Proxy
{
    internal Proxy(Dependency inner) {}
}

internal class Dependency
{
    internal Dependency(Proxy inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    private Container() {}
}