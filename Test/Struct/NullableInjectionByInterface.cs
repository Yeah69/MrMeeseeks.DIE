// ReSharper disable once CheckNamespace

using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Struct.NullableInjectionByInterface;

internal interface IInterface {}

internal struct Dependency : IInterface {}

internal class Root
{
    internal Root(IInterface? dependency) => Dependency = dependency;
    
    internal IInterface? Dependency { get; }
}

[CreateFunction(typeof(Root), "Create")]
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
        var value = container.Create();
        Assert.IsType<Root>(value);
        Assert.True(value.Dependency is not null);
    }
}