using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.CycleDetection.Implementation.Cycle.Proxied;


internal class Proxy
{
    // ReSharper disable once UnusedParameter.Local
    internal Proxy(Dependency inner) {}
}

internal class Dependency
{
    // ReSharper disable once UnusedParameter.Local
    internal Dependency(Proxy inner) {}
}

[CreateFunction(typeof(Dependency), "Create")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        Assert.Contains(DieExceptionKind.ImplementationCycle, container.ExceptionKinds_0_0);
    }
}