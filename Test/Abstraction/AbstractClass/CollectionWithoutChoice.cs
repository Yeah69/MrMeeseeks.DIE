using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Abstraction.AbstractClass.CollectionWithoutChoice;

internal abstract class Class;

internal sealed class SubClassA : Class;

internal sealed class SubClassB : Class;

[CreateFunction(typeof(IReadOnlyList<Class>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instances = container.Create();
        Assert.True(instances.Count == 2);
    }
}