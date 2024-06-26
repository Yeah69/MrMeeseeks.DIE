﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Abstraction.AbstractClass.SingleInCollection;

internal abstract class Class;

internal sealed class SubClassA : Class;

internal class SubClassB : Class;

[ImplementationCollectionChoice(typeof(Class), typeof(SubClassA))]
[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<SubClassA>(instance);
    }
}