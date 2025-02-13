using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Implementation.Choice.CollectionMultipleOfSameType;

internal class Class;

internal sealed class SubClassA : Class;

[ImplementationCollectionChoice(
    typeof(Class), 
    typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), 
    typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), 
    typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), 
    typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), typeof(SubClassA), 
    typeof(SubClassA), typeof(SubClassA))]
[CreateFunction(typeof(IReadOnlyList<Class>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instances = container.Create();
        Assert.True(instances is [SubClassA, SubClassA, SubClassA, SubClassA, SubClassA, SubClassA, SubClassA, 
            SubClassA, SubClassA, SubClassA, SubClassA, SubClassA, SubClassA, SubClassA, SubClassA, SubClassA, 
            SubClassA, SubClassA, SubClassA, SubClassA, SubClassA, SubClassA, SubClassA, SubClassA, SubClassA, 
            SubClassA]);
    }
}