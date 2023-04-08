using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.CycleDetection.Function.Cycle.IndirectRecursionContainer;

internal class Dependency : IContainerInstance
{
    internal Dependency(InnerDependency inner) {}
}

internal class InnerDependency : IContainerInstance
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