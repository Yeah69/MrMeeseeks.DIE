﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.BaseClassing.Class.InheritedUserDefinedFactory;

internal abstract class ContainerBase
{
    protected int DIE_Factory_Int => 69;
}

[CreateFunction(typeof(int), "Create")]
internal sealed partial class Container : ContainerBase;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var number = container.Create();
        Assert.Equal(69, number);
    }
}