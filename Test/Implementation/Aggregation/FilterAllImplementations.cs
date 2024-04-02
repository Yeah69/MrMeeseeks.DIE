using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Implementation.Aggregation.FilterAllImplementations;

internal interface IInterface;

internal class DependencyA : IInterface;

internal sealed class DependencyB : IInterface;

[FilterAllImplementationsAggregation]
[ImplementationAggregation(typeof(DependencyB))]
[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<DependencyB>(instance);
    }
}