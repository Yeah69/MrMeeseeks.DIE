﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Implementation.ArrayType;

internal class Dependency {}

[CreateFunction(typeof(Dependency[]), "Create")]
internal sealed partial class Container {}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var instance = container.Create();
        Assert.IsType<Dependency>(instance[0]);
    }
}