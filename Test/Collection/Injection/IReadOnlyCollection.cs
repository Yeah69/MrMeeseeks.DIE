using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Collection.Injection.IReadOnlyCollection;

internal interface IInterface;

internal sealed class ClassA : IInterface;

internal sealed class ClassB : IInterface;

internal sealed class ClassC : IInterface;

[CreateFunction(typeof(IReadOnlyCollection<IInterface>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var collection = container.Create();
        Assert.Equal(3, collection.Count);
    }
}