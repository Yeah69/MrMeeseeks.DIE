using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Choice.SingleWithSingleOtherSubstitute;

internal interface IInterface {}

internal class Class<T0> : IInterface {}

[GenericParameterSubstitutesChoice(typeof(Class<>), "T0", typeof(bool))]
[GenericParameterChoice(typeof(Class<>), "T0", typeof(int))]
[CreateFunction(typeof(IInterface), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var instance = container.Create();
        Assert.IsType<Class<int>>(instance);
    }
}