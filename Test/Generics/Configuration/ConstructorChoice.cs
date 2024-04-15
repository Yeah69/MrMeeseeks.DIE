using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Configuration.ConstructorChoiceFoo;

internal interface IInterface;

internal class DependencyA : IInterface;

internal sealed class DependencyB : IInterface;

// ReSharper disable once UnusedTypeParameter
internal sealed class Implementation<T0>
{
    public IInterface Dependency { get; }

    internal Implementation(DependencyA a) => Dependency = a;

    internal Implementation(DependencyB b) => Dependency = b;
}

[ConstructorChoice(typeof(Implementation<>), typeof(DependencyB))]
[CreateFunction(typeof(Implementation<int>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<DependencyB>(instance.Dependency);
    }
}