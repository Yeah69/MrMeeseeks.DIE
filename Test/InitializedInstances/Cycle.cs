using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;
// ReSharper disable UnusedParameter.Local

// ReSharper disable once CheckNamespace
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
    
    [InitializedInstances(typeof(DependencyA), typeof(DependencyB), typeof(DependencyC))]
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedType.Local
    // ReSharper disable once PartialTypeWithSinglePart
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
        Assert.Contains(DieExceptionKind.InitializedInstanceCycle, container.ExceptionKinds_0_0);
    }
}