using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Configuration.TransientScopeInstanceWithDifferentGenericParameter;

// ReSharper disable once UnusedTypeParameter
internal sealed class Class<T0> : ITransientScopeInstance;

internal sealed class TransientScopeRoot : ITransientScopeRoot
{
    public Class<int> Dependency0 { get; }
    public Class<string> Dependency1 { get; }

    internal TransientScopeRoot(
        Class<int> dependency0,
        Class<string> dependency1)
    {
        Dependency0 = dependency0;
        Dependency1 = dependency1;
    }
}

[CreateFunction(typeof(TransientScopeRoot), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var transientScopeRoot = container.Create();
        Assert.NotSame(transientScopeRoot.Dependency0, transientScopeRoot.Dependency1);
    }
}