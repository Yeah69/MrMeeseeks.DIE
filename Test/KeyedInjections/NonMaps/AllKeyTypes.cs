﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.KeyedInjections.NonMaps.AllKeyTypes;

internal enum Key
{
    A,
    B,
    C
}

internal interface IInterface;

[InjectionKey(Key.A)]
[InjectionKey(Byte)]
[InjectionKey(SByte)]
[InjectionKey(Short)]
[InjectionKey(UShort)]
[InjectionKey(Int)]
[InjectionKey(UInt)]
[InjectionKey(Long)]
[InjectionKey(ULong)]
[InjectionKey(Char)]
[InjectionKey(Float)]
[InjectionKey(Double)]
[InjectionKey(String)]
[InjectionKey(Bool)]
[InjectionKey(typeof(DependencyA))]
internal class DependencyA : IInterface
{
    internal const byte Byte = 0;
    internal const sbyte SByte = 1;
    internal const short Short = 2;
    internal const ushort UShort = 3;
    internal const int Int = 4;
    internal const uint UInt = 5;
    internal const long Long = 6;
    internal const ulong ULong = 7;
    internal const char Char = (char) 8;
    internal const float Float = 9.0f;
    internal const double Double = 10.0;
    internal const string String = "11";
    internal const bool Bool = true;
}

[InjectionKey(Key.B)]
[InjectionKey(Byte)]
[InjectionKey(SByte)]
[InjectionKey(Short)]
[InjectionKey(UShort)]
[InjectionKey(Int)]
[InjectionKey(UInt)]
[InjectionKey(Long)]
[InjectionKey(ULong)]
[InjectionKey(Char)]
[InjectionKey(Float)]
[InjectionKey(Double)]
[InjectionKey(String)]
[InjectionKey(Bool)]
[InjectionKey(typeof(DependencyB))]
internal sealed class DependencyB : IInterface
{
    internal const byte Byte = 12;
    internal const sbyte SByte = 13;
    internal const short Short = 14;
    internal const ushort UShort = 15;
    internal const int Int = 16;
    internal const uint UInt = 17;
    internal const long Long = 18;
    internal const ulong ULong = 19;
    internal const char Char = (char) 20;
    internal const float Float = 21.0f;
    internal const double Double = 22.0;
    internal const string String = "23";
    internal const bool Bool = false;
}


[InjectionKey(Key.C)]
[InjectionKey(Byte)]
[InjectionKey(SByte)]
[InjectionKey(Short)]
[InjectionKey(UShort)]
[InjectionKey(Int)]
[InjectionKey(UInt)]
[InjectionKey(Long)]
[InjectionKey(ULong)]
[InjectionKey(Char)]
[InjectionKey(Float)]
[InjectionKey(Double)]
[InjectionKey(String)]
[InjectionKey(Bool)]
[InjectionKey(typeof(DependencyC))]
internal class DependencyC : IInterface
{
    internal const byte Byte = 24;
    internal const sbyte SByte = 25;
    internal const short Short = 26;
    internal const ushort UShort = 27;
    internal const int Int = 28;
    internal const uint UInt = 29;
    internal const long Long = 30;
    internal const ulong ULong = 31;
    internal const char Char = (char) 32;
    internal const float Float = 33.0f;
    internal const double Double = 34.0;
    internal const string String = "35";
    internal const bool Bool = true;
}

internal sealed class Root
{
    [InjectionKey(Key.B)]
    public required IInterface DependencyEnum { get; init; }
    
    [InjectionKey(DependencyB.Byte)]
    public required IInterface DependencyByte { get; init; }
    
    [InjectionKey(DependencyB.SByte)]
    public required IInterface DependencySByte { get; init; }

    [InjectionKey(DependencyB.Short)]
    public required IInterface DependencyShort { get; init; }

    [InjectionKey(DependencyB.UShort)]
    public required IInterface DependencyUShort { get; init; }

    [InjectionKey(DependencyB.Int)]
    public required IInterface DependencyInt { get; init; }

    [InjectionKey(DependencyB.UInt)]
    public required IInterface DependencyUInt { get; init; }

    [InjectionKey(DependencyB.Long)]
    public required IInterface DependencyLong { get; init; }

    [InjectionKey(DependencyB.ULong)]
    public required IInterface DependencyULong { get; init; }

    [InjectionKey(DependencyB.Char)]
    public required IInterface DependencyChar { get; init; }

    [InjectionKey(DependencyB.Float)]
    public required IInterface DependencyFloat { get; init; }

    [InjectionKey(DependencyB.Double)]
    public required IInterface DependencyDouble { get; init; }

    [InjectionKey(DependencyB.String)]
    public required IInterface DependencyString { get; init; }

    [InjectionKey(DependencyB.Bool)]
    public required IInterface DependencyBool { get; init; }

    [InjectionKey(typeof(DependencyB))]
    public required IInterface DependencyType { get; init; }
}

[CreateFunction(typeof(Root), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var root = container.Create();
        Assert.IsType<DependencyB>(root.DependencyEnum);
        Assert.IsType<DependencyB>(root.DependencyByte);
        Assert.IsType<DependencyB>(root.DependencySByte);
        Assert.IsType<DependencyB>(root.DependencyShort);
        Assert.IsType<DependencyB>(root.DependencyUShort);
        Assert.IsType<DependencyB>(root.DependencyInt);
        Assert.IsType<DependencyB>(root.DependencyUInt);
        Assert.IsType<DependencyB>(root.DependencyLong);
        Assert.IsType<DependencyB>(root.DependencyULong);
        Assert.IsType<DependencyB>(root.DependencyChar);
        Assert.IsType<DependencyB>(root.DependencyFloat);
        Assert.IsType<DependencyB>(root.DependencyDouble);
        Assert.IsType<DependencyB>(root.DependencyString);
        Assert.IsType<DependencyB>(root.DependencyBool);
        Assert.IsType<DependencyB>(root.DependencyType);
    }
}