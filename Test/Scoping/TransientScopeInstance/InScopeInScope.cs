using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Scoping.TransientScopeInstance.InContainer.InScopeInScope;

internal interface IInterface {}

internal class Dependency : IInterface, ITransientScopeInstance {}

internal class ScopeRoot : IScopeRoot
{
    public IInterface Dependency { get; }
    public ScopeRoot(IInterface dependency) => Dependency = dependency;
}

internal class ScopeWithTransientScopeInstanceAbove : IScopeRoot
{
    public ScopeRoot InnerScope { get; }
    public ScopeWithTransientScopeInstanceAbove(ScopeRoot innerScope) => InnerScope = innerScope;
}

[CreateFunction(typeof(ScopeWithTransientScopeInstanceAbove), "Create")]
internal sealed partial class Container
{
    
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var scopeRoot = container.Create();
        Assert.IsType<Dependency>(scopeRoot.InnerScope.Dependency);
    }
}