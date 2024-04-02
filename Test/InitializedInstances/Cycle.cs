using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;
// ReSharper disable UnusedParameter.Local

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.InitializedInstances.Cycle;

internal sealed class DependencyA
{
    internal DependencyA(DependencyC c) {}
}

internal sealed class DependencyB
{
    internal DependencyB(DependencyA a) {}
}

internal sealed class DependencyC
{
    internal DependencyC(DependencyB b) {}
}

internal sealed class Root : IScopeRoot
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
    private sealed partial class DIE_DefaultScope;
}

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        Assert.Contains(DieExceptionKind.InitializedInstanceCycle, container.ExceptionKinds_0_0);
    }
}