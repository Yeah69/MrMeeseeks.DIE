using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Configuration.ConstructorChoiceFoo;

internal interface IInterface {}

internal class DependencyA : IInterface { }

internal class DependencyB : IInterface { }

internal class Implementation<T0>
{
    public IInterface Dependency { get; }

    internal Implementation(DependencyA a) => Dependency = a;

    internal Implementation(DependencyB b) => Dependency = b;
}

[ConstructorChoice(typeof(Implementation<>), typeof(DependencyB))]
[CreateFunction(typeof(Implementation<int>), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.IsType<DependencyB>(instance.Dependency);
    }
}