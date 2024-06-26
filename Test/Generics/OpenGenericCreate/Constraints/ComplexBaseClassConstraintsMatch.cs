
using System.Threading.Tasks;
using Xunit;
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.Constraints.ComplexBaseClassConstraintsMatch;

internal interface InnerInterface<T>;

internal class InnerClass<T> : InnerInterface<T>;

internal interface IInterface<T, T0> where T :  InnerClass<InnerInterface<T0>>, InnerInterface<InnerInterface<T0>>;

internal sealed class Class<T, T0> : IInterface<T, T0> where T : InnerClass<InnerInterface<T0>>;

[CreateFunction(typeof(IInterface<,>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create<InnerClass<InnerInterface<int>>, int>();
        Assert.IsType<Class<InnerClass<InnerInterface<int>>, int>>(instance);
    }
}