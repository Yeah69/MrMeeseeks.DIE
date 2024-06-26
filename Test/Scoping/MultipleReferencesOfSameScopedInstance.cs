using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;
// ReSharper disable UnusedParameter.Local

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.MultipleReferencesOfSameScopedInstance;

internal sealed class Dependency : IContainerInstance;

internal sealed class Parent
{
    internal Parent(
        Dependency dependency0,
        Dependency dependency1,
        Dependency dependency2,
        Dependency dependency3,
        Dependency dependency4,
        Dependency dependency5,
        Dependency dependency6,
        Dependency dependency7,
        Dependency dependency8,
        Dependency dependency9)
    {
        
    }
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Parent>(instance);
    }
}