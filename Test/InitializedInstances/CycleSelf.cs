using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;
// ReSharper disable UnusedParameter.Local

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.InitializedInstances.CycleSelf;

internal sealed class DependencyA
{
    internal DependencyA(DependencyA a) {}
}

internal sealed class Root : IScopeRoot
{
    internal Root(DependencyA a){}
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container
{
    
    [InitializedInstances(typeof(DependencyA))]
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