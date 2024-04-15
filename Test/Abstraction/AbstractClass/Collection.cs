using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Abstraction.AbstractClass.Collection;

internal abstract class Class;

internal sealed class SubClassA : Class;

internal sealed class SubClassB : Class;

internal class SubClassC : Class;

[ImplementationCollectionChoice(typeof(Class), typeof(SubClassA), typeof(SubClassB))]
[CreateFunction(typeof(IReadOnlyList<Class>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instances = container.Create();
        Assert.True(instances.Count == 2);
        Assert.True(instances[0] is SubClassA && instances[1] is SubClassB || instances[0] is SubClassB && instances[1] is SubClassA);
    }
}