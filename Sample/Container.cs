using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class DependencyA
{
    internal DependencyA(DependencyC c) {}
}

internal class DependencyB
{
    internal DependencyB(DependencyA a) {}
}

internal class DependencyC
{
    internal DependencyC(DependencyB b) {}
}

internal class Root : IScopeRoot
{
    internal Root(DependencyA a, DependencyB b, DependencyC c){}
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    private Container() {}
    
    [InitializedInstances(typeof(DependencyA), typeof(DependencyB), typeof(DependencyC))]
    private sealed partial class DIE_DefaultScope
    {
        
    }
}