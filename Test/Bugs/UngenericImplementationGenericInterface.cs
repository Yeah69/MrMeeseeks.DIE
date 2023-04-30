using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Bugs.UngenericImplementationGenericInterface;

// ReSharper disable once UnusedTypeParameter
internal interface IInterface<T> {}

internal class DependencyA : IInterface<int> {}

internal class DependencyB : IInterface<string> {}

internal class DependencyC : IInterface<long> {}

[CreateFunction(typeof(IInterface<string>), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<DependencyB>(instance);
    }
}