using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Choice.DoubleBothChosen;

internal interface IInterface;

// ReSharper disable UnusedTypeParameter
internal sealed class Class<T0, T1> : IInterface;
// ReSharper restore UnusedTypeParameter

[GenericParameterChoice(typeof(Class<,>), "T0", typeof(int))]
[GenericParameterChoice(typeof(Class<,>), "T1", typeof(string))]
[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Class<int, string>>(instance);
    }
}