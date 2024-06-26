﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.Interface.SingleAndBaseImplementationDoubleButOneFixed;

// ReSharper disable once UnusedTypeParameter
internal interface IInterface<T0>;

// ReSharper disable once UnusedTypeParameter
internal abstract class BaseClass<T0, T1> : IInterface<T0>;

internal sealed class Class<T0> : BaseClass<T0, string>;

[CreateFunction(typeof(IInterface<int>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instance = container.Create();
        Assert.IsType<Class<int>>(instance);
    }
}