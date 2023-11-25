using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.TransientScopeInstance.InContainer.InScopeInScope;

internal interface IInterface {}

internal class Dependency : IInterface, ITransientScopeInstance {}

internal class ScopeRoot(IInterface dependency) : IScopeRoot
{
    public IInterface Dependency { get; } = dependency;
}

internal class ScopeWithTransientScopeInstanceAbove(ScopeRoot innerScope) : IScopeRoot
{
    public ScopeRoot InnerScope { get; } = innerScope;
}

[CreateFunction(typeof(ScopeWithTransientScopeInstanceAbove), "Create")]
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
        var scopeRoot = container.Create();
        Assert.IsType<Dependency>(scopeRoot.InnerScope.Dependency);
    }
}