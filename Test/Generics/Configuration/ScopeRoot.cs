using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Configuration.ScopeRoot;

internal sealed class ScopeInstance : IScopeInstance;

// ReSharper disable once UnusedTypeParameter
internal sealed class ScopeRoot<T0> : IScopeRoot
{
    public ScopeInstance ScopeInstance { get; }

    internal ScopeRoot(ScopeInstance scopeInstance) => ScopeInstance = scopeInstance;
}

internal sealed class Root
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
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var root = container.Create();
        Assert.NotSame(root.ScopeInstance, root.ScopeRoot.ScopeInstance);
    }
}

