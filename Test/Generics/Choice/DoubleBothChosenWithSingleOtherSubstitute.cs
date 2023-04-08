using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Choice.DoubleBothChosenWithSingleOtherSubstitute;

internal interface IInterface {}

internal class Class<T0, T1> : IInterface {}

[GenericParameterSubstitutesChoice(typeof(Class<,>), "T0", typeof(uint))]
[GenericParameterChoice(typeof(Class<,>), "T0", typeof(int))]
[GenericParameterSubstitutesChoice(typeof(Class<,>), "T1", typeof(bool))]
[GenericParameterChoice(typeof(Class<,>), "T1", typeof(string))]
[CreateFunction(typeof(IInterface), "Create")]
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
        Assert.IsType<Class<int, string>>(instance);
    }
}