using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.ViaInitializedInstance;

internal sealed class Dependency : IContainerInstance;

internal sealed class ScopeRoot : IScopeRoot
{
    internal required Dependency Dependency { get; init; }
}

[InitializedInstances(typeof(Dependency))]
[CreateFunction(typeof(Dependency), "Create")]
[CreateFunction(typeof(ScopeRoot), "CreateScope")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        var scopeA = container.CreateScope();
        var scopeB = container.CreateScope();
        Assert.Same(instance, scopeA.Dependency);
        Assert.Same(instance, scopeB.Dependency);
    }
}