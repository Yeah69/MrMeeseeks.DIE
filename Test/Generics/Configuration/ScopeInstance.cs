using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.ScopeInstance;

internal class Class<T0> : ITransientScopeInstance { }

internal class ScopeRoot : ITransientScopeRoot
{
    public Class<int> Dependency0 { get; }
    public Class<int> Dependency1 { get; }

    internal ScopeRoot(
        Class<int> dependency0,
        Class<int> dependency1)
    {
        Dependency0 = dependency0;
        Dependency1 = dependency1;
    }
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var scopeRoot = container.Create();
        Assert.Same(scopeRoot.Dependency0, scopeRoot.Dependency1);
    }
}