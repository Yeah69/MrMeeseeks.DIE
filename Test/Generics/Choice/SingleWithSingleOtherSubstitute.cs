﻿using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Choice.SingleWithSingleOtherSubstitute;

internal interface IInterface;

// ReSharper disable once UnusedTypeParameter
internal class Class<T0> : IInterface;

[GenericParameterSubstitutesChoice(typeof(Class<>), "T0", typeof(bool))]
[GenericParameterChoice(typeof(Class<>), "T0", typeof(int))]
[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Class<int>>(instance);
    }
}