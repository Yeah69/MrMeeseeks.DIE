﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Interface.Double;

// ReSharper disable UnusedTypeParameter
internal interface IInterface<T0, T1>;
// ReSharper restore UnusedTypeParameter

internal sealed class Class<T0, T1> : IInterface<T0, T1>;

[CreateFunction(typeof(IInterface<int, string>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Class<int, string>>(instance);
    }
}