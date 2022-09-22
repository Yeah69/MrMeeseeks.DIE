using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.Sample;

namespace Asdf;

internal class Dependency : IScopeRoot
{
    internal Dependency(InnerDependency inner) {}
}

internal class InnerDependency : IScopeInstance
{
    internal InnerDependency(Dependency inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    
}