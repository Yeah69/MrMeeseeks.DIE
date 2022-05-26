using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Choice.DoubleBothChosenWithSingleOtherSubstitute;

internal interface IInterface {}

internal class Class<T0, T1> : IInterface {}

[GenericParameterSubstitutesChoice(typeof(Class<,>), "T0", typeof(uint))]
[GenericParameterChoice(typeof(Class<,>), "T0", typeof(int))]
[GenericParameterSubstitutesChoice(typeof(Class<,>), "T1", typeof(bool))]
[GenericParameterChoice(typeof(Class<,>), "T1", typeof(string))]
[CreateFunction(typeof(IInterface), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var instance = container.Create();
        Assert.IsType<Class<int, string>>(instance);
    }
}