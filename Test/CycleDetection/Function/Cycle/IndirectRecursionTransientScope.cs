using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CycleDetection.Function.Cycle.IndirectRecursionTransientScope;

internal class Dependency : ITransientScopeRoot
{
    internal Dependency(InnerDependency inner) {}
}

internal class InnerDependency : ITransientScopeInstance
{
    internal InnerDependency(Dependency inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        Assert.Equal(DieExceptionKind.FunctionCycle , container.ExceptionKind_0_0);
    }
}