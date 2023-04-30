using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.CycleDetection.Function.Cycle.DirectRecursionTransientScope;

internal class Dependency : ITransientScopeInstance
{
    // ReSharper disable once UnusedParameter.Local
    internal Dependency(Dependency inner) {}
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