using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Configuration.ConstructorChoiceFoo;

internal interface IInterface {}

internal class DependencyA : IInterface { }

internal class DependencyB : IInterface { }

// ReSharper disable once UnusedTypeParameter
internal class Implementation<T0>
{
    public IInterface Dependency { get; }

    internal Implementation(DependencyA a) => Dependency = a;

    internal Implementation(DependencyB b) => Dependency = b;
}

[ConstructorChoice(typeof(Implementation<>), typeof(DependencyB))]
[CreateFunction(typeof(Implementation<int>), "Create")]
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
        var instance = container.Create();
        Assert.IsType<DependencyB>(instance.Dependency);
    }
}