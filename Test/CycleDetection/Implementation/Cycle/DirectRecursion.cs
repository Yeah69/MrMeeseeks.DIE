using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CycleDetection.Implementation.Cycle.DirectRecursion;

internal class Dependency
{
    internal Dependency(Dependency inner) {}
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
        Assert.Equal(DieExceptionKind.ImplementationCycle , container.ExceptionKind_0_0);
    }
}