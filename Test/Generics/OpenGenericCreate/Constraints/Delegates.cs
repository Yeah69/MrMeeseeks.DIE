using System;
using Xunit;
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.Constraints.Delegates;

internal interface IInterface;

internal class Class : IInterface;

internal interface IInterface<T0>;

internal sealed class Dependency<T0> : IInterface<T0>;

internal interface IInterface<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> 
    where T0 : struct
    where T1 : class
    where T2 : class?
    where T3 : notnull
    where T4 : unmanaged
    where T5 : new()
    where T6 : Class
    where T7 : Class?
    where T8 : IInterface
    where T9 : IInterface?
{
    Func<IInterface<T0>> DependencyStruct { get; }
    Func<IInterface<T1>> DependencyClass { get; }
    Func<IInterface<T2>> DependencyNullableClass { get; }
    Func<IInterface<T3>> DependencyNotNull { get; }
    Func<IInterface<T4>> DependencyUnmanaged { get; }
    Func<IInterface<T5>> DependencyNew { get; }
    Func<IInterface<T6>> DependencyConcreteClass { get; }
    Func<IInterface<T7>> DependencyNullableConcreteClass { get; }
    Func<IInterface<T8>> DependencyInterface { get; }
    Func<IInterface<T9>> DependencyNullableInterface { get; }
}

internal sealed class DependencyHolder<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : IInterface<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> 
    where T0 : struct 
    where T1 : class
    where T2 : class?
    where T3 : notnull
    where T4 : unmanaged
    where T5 : new()
    where T6 : Class
    where T7 : Class?
    where T8 : IInterface
    where T9 : IInterface?
{
    public required Func<IInterface<T0>> DependencyStruct { get; init; }
    public required Func<IInterface<T1>> DependencyClass { get; init; }
    public required Func<IInterface<T2>> DependencyNullableClass { get; init; }
    public required Func<IInterface<T3>> DependencyNotNull { get; init; }
    public required Func<IInterface<T4>> DependencyUnmanaged { get; init; }
    public required Func<IInterface<T5>> DependencyNew { get; init; }
    public required Func<IInterface<T6>> DependencyConcreteClass { get; init; }
    public required Func<IInterface<T7>> DependencyNullableConcreteClass { get; init; }
    public required Func<IInterface<T8>> DependencyInterface { get; init; }
    public required Func<IInterface<T9>> DependencyNullableInterface { get; init; }
}

[CreateFunction(typeof(DependencyHolder<,,,,,,,,,>), "Create")]
[CreateFunction(typeof(IInterface<,,,,,,,,,>), "CreateInterface")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create<int, string, string?, int, byte, int, Class, Class?, Class, Class?>();
        Assert.IsType<DependencyHolder<int, string, string?, int, byte, int, Class, Class?, Class, Class?>>(instance);
        var interfaceInstance = container.CreateInterface<int, string, string?, int, byte, int, Class, Class?, Class, Class?>();
        Assert.IsType<DependencyHolder<int, string, string?, int, byte, int, Class, Class?, Class, Class?>>(interfaceInstance);
    }
}