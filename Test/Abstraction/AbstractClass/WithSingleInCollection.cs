﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Abstraction.AbstractClass.WithSingleInCollection;

internal abstract class Class {}

internal class SubClassA : Class {}

internal class SubClassB : Class {}

[ImplementationChoice(typeof(Class), typeof(SubClassA))]
[ImplementationCollectionChoice(typeof(Class), typeof(SubClassB))]
[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.IsType<SubClassA>(instance);
    }
}