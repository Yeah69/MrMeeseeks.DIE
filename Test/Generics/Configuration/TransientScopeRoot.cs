using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.TransientScopeRoot;

internal class ScopeInstance : IScopeInstance {}

internal class TransientScopeRoot<T0> : ITransientScopeRoot
{
    public ScopeInstance ScopeInstance { get; }

    internal TransientScopeRoot(ScopeInstance scopeInstance) => ScopeInstance = scopeInstance;
}

internal class Root
{
    public TransientScopeRoot<int> TransientScopeRoot { get; }
    public ScopeInstance ScopeInstance { get; }

    internal Root(
        TransientScopeRoot<int> transientScopeRoot,
        ScopeInstance scopeInstance)
    {
        TransientScopeRoot = transientScopeRoot;
        ScopeInstance = scopeInstance;
    }
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var root = container.Create();
        Assert.NotSame(root.ScopeInstance, root.TransientScopeRoot.ScopeInstance);
    }
}

