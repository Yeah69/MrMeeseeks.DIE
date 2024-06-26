using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.TestNotInternalsVisibleToChild.Public;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Implementation.Aggregation.AssemblyImplementationsAggregation;

[FilterAllImplementationsAggregation]
[AssemblyImplementationsAggregation(typeof(MrMeeseeks.DIE.TestNotInternalsVisibleToChild.AssemblyInfo))]
[ConstructorChoice(typeof(Parent.ClassToo))]
[CreateFunction(typeof(Parent.ClassToo), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Parent.ClassToo>(instance);
    }
}