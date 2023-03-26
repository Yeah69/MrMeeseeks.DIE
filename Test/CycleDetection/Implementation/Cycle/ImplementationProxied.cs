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
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        Assert.True(container.ExceptionKinds_0_0.Contains(DieExceptionKind.ImplementationCycle));
    }
}