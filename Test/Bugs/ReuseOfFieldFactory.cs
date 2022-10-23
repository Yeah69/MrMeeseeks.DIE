using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Bugs.ReuseOfFieldFactory;

internal interface IInterface {}

internal class Dependency : IInterface {}

internal class DependencyHolder
{
    public IInterface Dependency { get; }
    internal DependencyHolder(IInterface dependency)
    {
        Dependency = dependency;
    }
}

[CreateFunction(typeof(DependencyHolder), "CreateHolder")]
[CreateFunction(typeof(IInterface), "CreateInterface")]
internal sealed partial class Container
{
    private readonly IInterface DIE_Factory_dependency;

    internal Container(IInterface dependency)
    {
        DIE_Factory_dependency = dependency;
    }
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var originalDependency = new Dependency();
        using var container = new Container(originalDependency);
        var holder = container.CreateHolder();
        Assert.Same(originalDependency, holder.Dependency);
        var instance = container.CreateInterface();
        Assert.Same(originalDependency, instance);
    }
}