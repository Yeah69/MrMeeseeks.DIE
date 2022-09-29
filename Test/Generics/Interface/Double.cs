using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Interface.Double;

internal interface IInterface<T0, T1> {}

internal class Class<T0, T1> : IInterface<T0, T1> {}

[CreateFunction(typeof(IInterface<int, string>), "Create")]
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