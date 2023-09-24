using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.NonMaps.SingleInitParamSimpleChoice;

internal enum Key
{
    A,
    B,
    C
}

internal interface IInterface
{
}

internal class DependencyA : IInterface
{
}

internal class DependencyB : IInterface
{
}

internal class DependencyC : IInterface
{
}

internal class Root
{
    internal IInterface? Dependency { get; private set; }

    public void Initialize([Key(Key.B)] IInterface dependency) => Dependency = dependency;
}

[InjectionKeyChoice(Key.A, typeof(DependencyA))]
[InjectionKeyChoice(Key.B, typeof(DependencyB))]
[InjectionKeyChoice(Key.C, typeof(DependencyC))]
[Initializer(typeof(Root), nameof(Root.Initialize))]
[CreateFunction(typeof(Root), "Create")]
internal partial class Container
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