using System;
using Xunit;
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.Constraints.AbstractionConstraintsMatch;

internal interface IInterface {}
internal class BaseClass : IInterface {}
internal interface IInterfaceStruct<T0> where T0 : struct { }
internal interface IInterfaceClass<T1> where T1 : class { }
internal interface IInterfaceNullableClass<T2> where T2 : class? { }
internal interface IInterfaceNotNull<T3> where T3 : notnull { }
internal interface IInterfaceUnmanaged<T4> where T4 : unmanaged { }
internal interface IInterfaceNew<T5> where T5 : new() { }
internal interface IInterfaceBaseClass<T6> where T6 : BaseClass { }
internal interface IInterfaceNullableBaseClass<T7> where T7 : BaseClass? { }
internal interface IInterfaceInterface<T8> where T8 : IInterface { }
internal interface IInterfaceNullableInterface<T9> where T9 : IInterface? { }
internal class DependencyStruct<T0> : IInterfaceStruct<T0> where T0 : struct { }
internal class DependencyClass<T1> : IInterfaceClass<T1> where T1 : class { }
internal class DependencyNullableClass<T2> : IInterfaceNullableClass<T2> where T2 : class? { }
internal class DependencyNotNull<T3> : IInterfaceNotNull<T3> where T3 : notnull { }
internal class DependencyUnmanaged<T4> : IInterfaceUnmanaged<T4> where T4 : unmanaged { }
internal class DependencyNew<T5> : IInterfaceNew<T5> where T5 : new() { }
internal class DependencyBaseClass<T6> : IInterfaceBaseClass<T6> where T6 : BaseClass { }
internal class DependencyNullableBaseClass<T7> : IInterfaceNullableBaseClass<T7> where T7 : BaseClass? { }
internal class DependencyInterface<T8> : IInterfaceInterface<T8> where T8 : IInterface { }
internal class DependencyNullableInterface<T9> : IInterfaceNullableInterface<T9> where T9 : IInterface? { }

internal class Proxy<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> 
    where T0 : struct
    where T1 : class
    where T2 : class?
    where T3 : notnull
    where T4 : unmanaged
    where T5 : new()
    where T6 : BaseClass
    where T7 : BaseClass?
    where T8 : IInterface
    where T9 : IInterface?
{
    internal required IInterfaceStruct<T0> DependencyStruct { get; init; } 
    internal required IInterfaceClass<T1> DependencyClass { get; init; }
    internal required IInterfaceNullableClass<T2> DependencyNullableClass { get; init; } 
    internal required IInterfaceNotNull<T3> DependencyNotNull { get; init; } 
    internal required IInterfaceUnmanaged<T4> DependencyUnmanaged { get; init; }
    internal required IInterfaceNew<T5> DependencyNew { get; init; }
    internal required IInterfaceBaseClass<T6> DependencyBaseClass { get; init; } 
    internal required IInterfaceNullableBaseClass<T7> DependencyNullableBaseClass { get; init; }
    internal required IInterfaceInterface<T8> DependencyInterface { get; init; }
    internal required IInterfaceNullableInterface<T9> DependencyNullableInterface { get; init; }
}

[CreateFunction(typeof(Proxy<,,,,,,,,,>), "Create")]
internal sealed partial class Container { }

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        _ = container.Create<int, BaseClass, IInterface?, DateTime, long, byte, BaseClass, BaseClass?, IInterface, IInterface?>();
    }
}