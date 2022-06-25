using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Interface.DoubleAndOneFixed;

internal interface IInterface<T0, T1> {}

internal class Class<T0> : IInterface<T0, string> {}

[CreateFunction(typeof(IInterface<int, string>), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.IsType<Class<int>>(instance);
    }
}