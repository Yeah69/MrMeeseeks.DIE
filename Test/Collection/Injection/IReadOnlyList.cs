﻿using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Collection.Injection.IReadOnlyList;

internal interface IInterface {}

internal class ClassA : IInterface {}

internal class ClassB : IInterface {}

internal class ClassC : IInterface {}

[CreateFunction(typeof(IReadOnlyList<IInterface>), "Create")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var collection = container.Create();
        Assert.Equal(3, collection.Count);
    }
}