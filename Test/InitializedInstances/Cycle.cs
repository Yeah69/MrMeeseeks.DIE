using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.InitializedInstances.Cycle;

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
    [InitializedInstancesForScopes(typeof(DependencyA), typeof(DependencyB), typeof(DependencyC))]
    private sealed partial class DIE_DefaultScope
    {
        
    }
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        Assert.Equal(DieExceptionKind.InitializedInstanceCycle, container.ExceptionKind_0_0);
    }
}