using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.NonMaps.SingleConstrParamSimple;

internal enum Key
{
    A,
    B,
    C
}

internal interface IInterface;

[InjectionKey(Key.A)]
internal class DependencyA : IInterface;

[InjectionKey(Key.B)]
internal sealed class DependencyB : IInterface;

[InjectionKey(Key.C)]
internal class DependencyC : IInterface;

internal sealed class Root
{
    public Root([InjectionKey(Key.B)] IInterface dependency) => Dependency = dependency;

    public IInterface Dependency { get; }
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
        Assert.IsType<DependencyB>(root.Dependency);
    }
}