using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Implementation.Aggregation.FilterAllImplementations;

internal interface IInterface { }

internal class DependencyA : IInterface {}

internal class DependencyB : IInterface {}

[FilterAllImplementationsAggregation]
[ImplementationAggregation(typeof(DependencyB))]
[CreateFunction(typeof(IInterface), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var instance = container.Create();
        Assert.IsType<DependencyB>(instance);
    }
}