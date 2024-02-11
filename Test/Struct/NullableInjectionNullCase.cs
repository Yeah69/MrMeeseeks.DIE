// ReSharper disable once CheckNamespace

using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Struct.NullableInjectionNullCase;

internal struct Dependency {}

internal class Root
{
    internal Root(Dependency? dependency) => Dependency = dependency;
    
    internal Dependency? Dependency { get; }
}

[FilterImplementationAggregation(typeof(Dependency))]
[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var value = container.Create();
        Assert.IsType<Root>(value);
        Assert.False(value.Dependency.HasValue);
    }
}