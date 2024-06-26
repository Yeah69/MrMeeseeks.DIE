﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Implementation.Choice.SingleInCollection;

internal class Class;

internal sealed class SubClass : Class;

[ImplementationCollectionChoice(typeof(Class), typeof(SubClass))]
[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<SubClass>(instance);
    }
}