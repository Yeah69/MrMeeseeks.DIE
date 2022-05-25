using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.Test.CycleDetection.Implementation;

internal class Dependency
{
    internal Dependency(Dependency inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal partial class Container
{
    
}