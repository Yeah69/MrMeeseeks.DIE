using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Test.CycleDetection.Implementation.Cycle.Proxied;


internal class Proxy1
{
    internal Proxy1(Dependency inner) {}
}
internal class Proxy0
{
    internal Proxy0(Proxy1 inner) {}
}

internal class Dependency
{
    internal Dependency(Proxy0 inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    
}