﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Abstraction.AbstractClass.VanillaWithoutChoice;

internal abstract class Class {}

internal class SubClass : Class {}

[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container {}

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