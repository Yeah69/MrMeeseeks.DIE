using System;
using MrMeeseeks.DIE.Configuration;
using Xunit;

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
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var scopeRoot = container.Create();
        Assert.NotSame(scopeRoot.Dependency0, scopeRoot.Dependency1);
    }
}