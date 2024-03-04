﻿using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Choice.Double;

// ReSharper disable once UnusedTypeParameter
internal interface IInterface<T0>;

// ReSharper disable once UnusedTypeParameter
internal class Class<T0, T1> : IInterface<T0>;

[GenericParameterChoice(typeof(Class<,>), "T1", typeof(string))]
[CreateFunction(typeof(IInterface<int>), "Create")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Class<int, string>>(instance);
    }
}