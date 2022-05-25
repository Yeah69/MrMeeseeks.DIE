using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.CycleDetection.ImplementationProxied;


internal class Proxy
{
    internal Proxy(Dependency inner) {}
}

internal class Dependency
{
    internal Dependency(Proxy inner) {}
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
        Assert.Equal(DieExceptionKind.ImplementationCycle , container.ExceptionKind_0_0);
    }
}