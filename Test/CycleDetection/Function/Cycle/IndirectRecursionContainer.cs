using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.CycleDetection.Function.Cycle.IndirectRecursionContainer;

internal class Dependency : IContainerInstance
{
    // ReSharper disable once UnusedParameter.Local
    internal Dependency(InnerDependency inner) {}
}

internal class InnerDependency : IContainerInstance
{
    // ReSharper disable once UnusedParameter.Local
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
        Assert.Contains(DieExceptionKind.FunctionCycle, container.ExceptionKinds_0_0);
    }
}