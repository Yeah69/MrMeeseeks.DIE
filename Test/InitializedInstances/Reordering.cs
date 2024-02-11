using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;
// ReSharper disable UnusedParameter.Local

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.InitializedInstances.Reordering;

internal class DependencyA
{
    internal DependencyA(DependencyC c, DependencyB b) {}
}

internal class DependencyB
{
    internal DependencyB(DependencyC c) {}
}

internal class DependencyC { }

internal class Root : IScopeRoot
{
    internal Root(DependencyA a, DependencyB b, DependencyC c){}
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    
    [InitializedInstances(typeof(DependencyA), typeof(DependencyB), typeof(DependencyC))]
    // ReSharper disable once InconsistentNaming
    private sealed partial class DIE_DefaultScope
    {
        
    }
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var _ = container.Create();
    }
}