using System.Threading;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.ThreadLocal.Vanilla;

internal class Dependency{}

[CreateFunction(typeof(ThreadLocal<Dependency>), "Create")]
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
        var lazy = container.Create();
        var _ = lazy.Value;
    }
}
