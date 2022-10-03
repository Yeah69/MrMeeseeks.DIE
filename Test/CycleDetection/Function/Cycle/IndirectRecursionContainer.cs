using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

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