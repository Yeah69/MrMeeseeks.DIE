using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.TransientScopeInstance;

internal class Class<T0> : ITransientScopeInstance { }

internal class TransientScopeRoot : ITransientScopeRoot
{
    public Class<int> Dependency0 { get; }
    public Class<int> Dependency1 { get; }

    internal TransientScopeRoot(
        Class<int> dependency0,
        Class<int> dependency1)
    {
        Dependency0 = dependency0;
        Dependency1 = dependency1;
    }
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var transientScopeRoot = container.Create();
        Assert.Same(transientScopeRoot.Dependency0, transientScopeRoot.Dependency1);
    }
}