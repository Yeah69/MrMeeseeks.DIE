using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Choice.DoubleWithSingleOtherSubstitute;

internal interface IInterface<T0> {}

internal class Class<T0, T1> : IInterface<T0> {}

[GenericParameterSubstitutesChoice(typeof(Class<,>), "T1", typeof(bool))]
[GenericParameterChoice(typeof(Class<,>), "T1", typeof(string))]
[CreateFunction(typeof(IInterface<int>), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = container.Create();
        Assert.IsType<Class<int, string>>(instance);
    }
}