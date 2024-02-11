using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Interface.DoubleAndSetToSame;

// ReSharper disable UnusedTypeParameter
internal interface IInterface<T0, T1> {}
// ReSharper restore UnusedTypeParameter

internal class Class<T0> : IInterface<T0, T0> {}

[CreateFunction(typeof(IInterface<int, int>), "Create")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Class<int>>(instance);
    }
}