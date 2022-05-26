using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Interface.DoubleAndOneFixed;

internal interface IInterface<T0, T1> {}

internal class Class<T0> : IInterface<T0, string> {}

[CreateFunction(typeof(IInterface<int, string>), "Create")]
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