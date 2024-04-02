using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.NonMaps.SingleKeyNotUsed;

internal enum Key
{
    A,
    B
}

internal interface IInterface;

[InjectionKey(Key.B)]
internal class DependencyB : IInterface;

internal sealed class Root
{
    public Root([InjectionKey(Key.A)] IInterface? dependency) => Dependency = dependency;

    public IInterface? Dependency { get; }
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var root = container.Create();
        Assert.Null(root.Dependency);
    }
}