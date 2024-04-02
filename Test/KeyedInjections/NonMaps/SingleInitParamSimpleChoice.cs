using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.NonMaps.SingleInitParamSimpleChoice;

internal enum Key
{
    A,
    B,
    C
}

internal interface IInterface;

internal sealed class DependencyA : IInterface;

internal sealed class DependencyB : IInterface;

internal sealed class DependencyC : IInterface;

internal sealed class Root
{
    internal IInterface? Dependency { get; private set; }

    public void Initialize([InjectionKey(Key.B)] IInterface dependency) => Dependency = dependency;
}

[InjectionKeyChoice(Key.A, typeof(DependencyA))]
[InjectionKeyChoice(Key.B, typeof(DependencyB))]
[InjectionKeyChoice(Key.C, typeof(DependencyC))]
[Initializer(typeof(Root), nameof(Root.Initialize))]
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