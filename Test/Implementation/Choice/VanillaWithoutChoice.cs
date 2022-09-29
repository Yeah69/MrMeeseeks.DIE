﻿using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Implementation.Choice.VanillaWithoutChoice;

internal class Class {}

internal class SubClass : Class {}

[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var instance = container.Create();
        Assert.IsType<Class>(instance);
    }
}