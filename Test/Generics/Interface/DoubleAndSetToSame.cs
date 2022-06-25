using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Interface.DoubleAndSetToSame;

internal interface IInterface<T0, T1> {}

internal class Class<T0> : IInterface<T0, T0> {}

[CreateFunction(typeof(IInterface<int, int>), "Create")]
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