﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Implementation.Choice.SingleInCollection;

internal class Class {}

internal class SubClass : Class {}

[ImplementationCollectionChoice(typeof(Class), typeof(SubClass))]
[CreateFunction(typeof(Class), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.IsType<SubClass>(instance);
    }
}