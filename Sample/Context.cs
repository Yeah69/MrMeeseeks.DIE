using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Test.CycleDetection.Implementation;

internal class Dependency
{
    internal Dependency(Dependency inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal partial class Container
{
    
}