using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Configuration.ScopeInstanceWithDifferentGenericParameter;

internal class Class<T0> : IScopeInstance { }

internal class ScopeRoot : IScopeRoot
{
    public Class<int> Dependency0 { get; }
    public Class<string> Dependency1 { get; }

    internal ScopeRoot(
        Class<int> dependency0,
        Class<string> dependency1)
    {
        Dependency0 = dependency0;
        Dependency1 = dependency1;
    }
}

[CreateFunction(typeof(ScopeRoot), "Create")]
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
        Assert.NotSame(scopeRoot.Dependency0, scopeRoot.Dependency1);
    }
}