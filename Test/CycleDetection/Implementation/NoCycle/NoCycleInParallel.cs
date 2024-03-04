using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.CycleDetection.Implementation.InParallel;

internal class Dependency;

internal class Parent
{
    internal Parent(
        // ReSharper disable once UnusedParameter.Local
        Dependency dep0,
        // ReSharper disable once UnusedParameter.Local
        Dependency dep1)
    {
        
    }
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var parent = container.Create();
        Assert.IsType<Parent>(parent);
    }
}