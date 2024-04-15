using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Scoping.TransientScopeInstance.InContainer.InScopeInScope;

internal interface IInterface;

internal sealed class Dependency : IInterface, ITransientScopeInstance;

internal sealed class ScopeRoot : IScopeRoot
{
    public IInterface Dependency { get; }
    public ScopeRoot(IInterface dependency) => Dependency = dependency;
}

internal sealed class ScopeWithTransientScopeInstanceAbove : IScopeRoot
{
    public ScopeRoot InnerScope { get; }
    public ScopeWithTransientScopeInstanceAbove(ScopeRoot innerScope) => InnerScope = innerScope;
}

[CreateFunction(typeof(ScopeWithTransientScopeInstanceAbove), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var scopeRoot = container.Create();
        Assert.IsType<Dependency>(scopeRoot.InnerScope.Dependency);
    }
}