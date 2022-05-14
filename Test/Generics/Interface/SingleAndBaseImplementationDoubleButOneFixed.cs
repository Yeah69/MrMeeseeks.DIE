﻿using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Generics.Interface.SingleAndBaseImplementationDoubleButOneFixed;

internal interface IInterface<T0> {}

internal abstract class BaseClass<T0, T1> : IInterface<T0> {}

internal class Class<T0> : BaseClass<T0, string> {}

[CreateFunction(typeof(IInterface<int>), "Create")]
internal partial class Container {}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var instance = container.Create();
        Assert.IsType<Class<int>>(instance);
    }
}