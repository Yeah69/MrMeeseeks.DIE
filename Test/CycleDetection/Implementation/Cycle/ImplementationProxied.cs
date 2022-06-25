using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.CycleDetection.Implementation.Cycle.Proxied;


internal class Proxy
{
    internal Proxy(Dependency inner) {}
}

internal class Dependency
{
    internal Dependency(Proxy inner) {}
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