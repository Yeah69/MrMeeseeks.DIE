// ReSharper disable once CheckNamespace

using MrMeeseeks.DIE.Configuration.Attributes;
using System.Threading.Tasks;
using Xunit;

namespace MrMeeseeks.DIE.Test.Struct.NullableInjectionNullCase;

internal struct Dependency;

internal sealed class Root
{
    internal Root(Dependency? dependency) => Dependency = dependency;
    
    internal Dependency? Dependency { get; }
}

[FilterImplementationAggregation(typeof(Dependency))]
[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var value = container.Create();
        Assert.IsType<Root>(value);
        Assert.False(value.Dependency.HasValue);
    }
}