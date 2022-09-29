using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.ChoiceIndirectlyBySingleSubstitute.Single;

internal interface IInterface {}

internal class Class<T0> : IInterface {}

[GenericParameterSubstitutesChoice(typeof(Class<>), "T0", typeof(int))]
[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = container.Create();
        Assert.IsType<Class<int>>(instance);
    }
}