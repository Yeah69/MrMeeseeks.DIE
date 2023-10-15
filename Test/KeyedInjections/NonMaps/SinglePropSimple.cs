using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.NonMaps.SinglePropSimple;

internal enum Key
{
    A,
    B,
    C
}

internal interface IInterface
{
}

[InjectionKey(Key.A)]
internal class DependencyA : IInterface
{
}

[InjectionKey(Key.B)]
internal class DependencyB : IInterface
{
}

[InjectionKey(Key.C)]
internal class DependencyC : IInterface
{
}

internal class Root
{
    [InjectionKey(Key.B)]
    internal required IInterface Dependency { get; init; }
}

[CreateFunction(typeof(Root), "Create")]
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
        var root = container.Create();
        Assert.IsType<DependencyB>(root.Dependency);
    }
}