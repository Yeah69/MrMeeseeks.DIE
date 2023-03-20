using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class DependencyA : ITaskInitializer
{
    internal DependencyA() {}
    public Task InitializeAsync() => 
        Task.CompletedTask;
}

internal class DependencyB
{
    internal DependencyB(DependencyC c) {}
}

internal class DependencyC
{
    internal DependencyC(DependencyA a) {}
}

internal class Root : IScopeRoot
{
    internal Root(DependencyA a, DependencyB b, DependencyC c){}
}

[CreateFunction(typeof(Root), "Create")]
[InitializedInstances(typeof(DependencyA), typeof(DependencyB), typeof(DependencyC))]
internal sealed partial class Container
{
    [InitializedInstances(typeof(DependencyA), typeof(DependencyB), typeof(DependencyC))]
    private sealed partial class DIE_DefaultScope
    {
        
    }
}
