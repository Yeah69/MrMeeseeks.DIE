using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Bugs.ReuseOfFieldFactory;

internal interface IInterface;

internal sealed class Dependency : IInterface;

internal sealed class DependencyHolder
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
    // ReSharper disable once InconsistentNaming
    private readonly IInterface DIE_Factory_dependency;
    
    private Container(IInterface dependency)
    {
        DIE_Factory_dependency = dependency;
    }
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        var originalDependency = new Dependency();
        await using var container = Container.DIE_CreateContainer(originalDependency);
        var holder = container.CreateHolder();
        Assert.Same(originalDependency, holder.Dependency);
        var instance = container.CreateInterface();
        Assert.Same(originalDependency, instance);
    }
}