using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.NonMaps.SingleInitParamSimple;

internal enum Key
{
    A,
    B,
    C
}

internal interface IInterface
{
}

[Key(Key.A)]
internal class DependencyA : IInterface
{
}

[Key(Key.B)]
internal class DependencyB : IInterface
{
}

[Key(Key.C)]
internal class DependencyC : IInterface
{
}

internal class Root
{
    internal IInterface? Dependency { get; private set; }

    public void Initialize([Key(Key.B)] IInterface dependency) => Dependency = dependency;
}

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