using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Interface.Single;

internal interface IInterface<T0> {}

internal class Class<T0> : IInterface<T0> {}

[CreateFunction(typeof(IInterface<int>), "Create")]
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