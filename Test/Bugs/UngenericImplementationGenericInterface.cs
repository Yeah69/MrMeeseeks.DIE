using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Bugs.UngenericImplementationGenericInterface;

internal interface IInterface<T> {}

internal class DependencyA : IInterface<int> {}

internal class DependencyB : IInterface<string> {}

internal class DependencyC : IInterface<long> {}

[CreateFunction(typeof(IInterface<string>), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = container.Create();
        Assert.IsType<DependencyB>(instance);
    }
}