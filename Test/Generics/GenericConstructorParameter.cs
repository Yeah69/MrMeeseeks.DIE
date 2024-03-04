

using Xunit;
// ReSharper disable once CheckNamespace
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.GenericConstructorParameter;

internal record Dependency;

internal class DependencyHolder<T0>
{
    // ReSharper disable once UnusedParameter.Local
    internal DependencyHolder(T0 _) {}
}

[CreateFunction(typeof(DependencyHolder<Dependency>), "Create")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<DependencyHolder<Dependency>>(instance);
    }
}