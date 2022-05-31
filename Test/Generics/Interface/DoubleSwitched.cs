using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Interface.DoubleSwitched;

internal interface IInterface<T0, T1> {}

internal class Class<T1, T0> : IInterface<T0, T1> {}

[CreateFunction(typeof(IInterface<int, string>), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.IsType<Class<string, int>>(instance);
    }
}