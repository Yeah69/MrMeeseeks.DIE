using System;
using System.IO;
using Xunit;
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.TopLevelOnly;

internal interface IInterface<T0>;

internal interface IInterface<T0, T1, T2, T3, T4>;

internal sealed class Class<T0> : IInterface<T0>;

internal sealed class Class<T0, T1, T2, T3, T4> : IInterface<T0, T1, T2, T3, T4>;

[CreateFunction(typeof(Class<>), "Create")]
[CreateFunction(typeof(Class<,,,,>), "CreateMulti")]
[CreateFunction(typeof(IInterface<>), "CreateInterface")]
[CreateFunction(typeof(IInterface<,,,,>), "CreateInterfaceMulti")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create<string>();
        var instanceMulti = container.CreateMulti<string, object, int, double, string>();
        Assert.IsType<Class<string>>(instance);
        Assert.IsType<Class<string, object, int, double, string>>(instanceMulti);
        var interfaceInstance = container.CreateInterface<DateTime>();
        var interfaceInstanceMulti = container.CreateInterfaceMulti<uint, FileInfo, IDisposable, Func<string>, int>();
        Assert.IsType<Class<DateTime>>(interfaceInstance);
        Assert.IsType<Class<uint, FileInfo, IDisposable, Func<string>, int>>(interfaceInstanceMulti);
    }
}