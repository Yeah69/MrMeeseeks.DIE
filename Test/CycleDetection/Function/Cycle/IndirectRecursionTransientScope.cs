using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.CycleDetection.Function.Cycle.IndirectRecursionTransientScope;

internal sealed class Dependency : ITransientScopeRoot
{
    // ReSharper disable once UnusedParameter.Local
    internal Dependency(InnerDependency inner) {}
}

internal class InnerDependency : ITransientScopeInstance
{
    // ReSharper disable once UnusedParameter.Local
    internal InnerDependency(Dependency inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        Assert.Contains(DieExceptionKind.FunctionCycle, container.ExceptionKinds_0_0);
    }
}