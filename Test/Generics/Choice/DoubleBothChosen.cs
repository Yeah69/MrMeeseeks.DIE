using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Choice.DoubleBothChosen;

internal interface IInterface {}

internal class Class<T0, T1> : IInterface {}

[GenericParameterChoice(typeof(Class<,>), "T0", typeof(int))]
[GenericParameterChoice(typeof(Class<,>), "T1", typeof(string))]
[CreateFunction(typeof(IInterface), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.IsType<Class<int, string>>(instance);
    }
}