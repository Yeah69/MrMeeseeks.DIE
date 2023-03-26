using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CycleDetection.Function.Cycle.IndirectRecursionScope;

internal class Dependency : IScopeRoot
{
    internal Dependency(InnerDependency inner) {}
}

internal class InnerDependency : IScopeInstance
{
    internal InnerDependency(Dependency inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        Assert.True(container.ExceptionKinds_0_0.Contains(DieExceptionKind.FunctionCycle));
    }
}