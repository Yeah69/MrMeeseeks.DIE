using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CycleDetection.Function.Cycle.DirectRecursionScope;

internal class Dependency : IScopeInstance
{
    internal Dependency(Dependency inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal partial class Container
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