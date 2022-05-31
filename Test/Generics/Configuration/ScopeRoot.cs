using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.ScopeRoot;

internal class ScopeInstance : IScopeInstance {}

internal class ScopeRoot<T0> : IScopeRoot
{
    public ScopeInstance ScopeInstance { get; }

    internal ScopeRoot(ScopeInstance scopeInstance) => ScopeInstance = scopeInstance;
}

internal class Root
{
    public ScopeRoot<int> ScopeRoot { get; }
    public ScopeInstance ScopeInstance { get; }

    internal Root(
        ScopeRoot<int> scopeRoot,
        ScopeInstance scopeInstance)
    {
        ScopeRoot = scopeRoot;
        ScopeInstance = scopeInstance;
    }
}

[CreateFunction(typeof(Root), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var root = container.Create();
        Assert.NotSame(root.ScopeInstance, root.ScopeRoot.ScopeInstance);
    }
}

