using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Scoping.TransientScopeInstance.InContainer.InScope;

internal interface IInterface {}

internal class Dependency : IInterface, ITransientScopeInstance {}

internal class ScopeRoot : IScopeRoot
{
    public IInterface Dependency { get; }
    public ScopeRoot(IInterface dependency) => Dependency = dependency;
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal partial class Container
{
    
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var scopeRoot = container.Create();
        Assert.IsType<Dependency>(scopeRoot.Dependency);
    }
}