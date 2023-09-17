using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal enum Key
{
    A,
    B,
    C
}

internal interface IInterface
{
}

[Key(Key.A)]
/*[Key((byte)0)]
[Key((sbyte)1)]
[Key((short)2)]
[Key((ushort)3)]
[Key((int)4)]
[Key((uint)5)]
[Key((long)6)]
[Key((ulong)7)]
[Key((char)8)]
[Key((float)9.0)]
[Key((double)10.0)]
[Key((decimal)11)]
[Key("12")]
[Key(true)]*/
internal class DependencyA : IInterface
{
}

[Key(Key.B)]
[Key(typeof(DependencyB))]
/*[Key((byte)13)]
[Key((sbyte)14)]
[Key((short)15)]
[Key((ushort)16)]
[Key((int)17)]
[Key((uint)18)]
[Key((long)19)]
[Key((ulong)20)]
[Key((char)21)]
[Key((float)22.0)]
[Key((double)23.0)]
[Key((decimal)24)]
[Key("25")]
[Key(false)]*/
internal class DependencyB : IInterface
{
    internal const byte Byte = 13;
    internal const sbyte SByte = 14;
}


[Key(Key.C)]
/*[Key((byte)26)]
[Key((sbyte)27)]
[Key((short)28)]
[Key((ushort)29)]
[Key((int)30)]
[Key((uint)31)]
[Key((long)32)]
[Key((ulong)33)]
[Key((char)34)]
[Key((float)35.0)]
[Key((double)36.0)]
[Key((decimal)37)]
[Key("38")]
[Key(true)]*/
internal class DependencyC : IInterface
{
}

internal class Root
{
    [Key(Key.B)]
    public required IInterface DependencyEnum { get; init; }
    
    [Key(typeof(DependencyB))]
    public required IInterface DependencyByte { get; init; }
    
    /*[Key((sbyte)14)]
    public required IInterface DependencySByte { get; init; }

    [Key((short)15)]
    public required IInterface DependencyShort { get; init; }

    [Key((ushort)16)]
    public required IInterface DependencyUShort { get; init; }

    [Key((int)17)]
    public required IInterface DependencyInt { get; init; }

    [Key((uint)18)]
    public required IInterface DependencyUInt { get; init; }

    [Key((long)19)]
    public required IInterface DependencyLong { get; init; }

    [Key((ulong)20)]
    public required IInterface DependencyULong { get; init; }

    [Key((char)21)]
    public required IInterface DependencyChar { get; init; }

    [Key((float)22.0)]
    public required IInterface DependencyFloat { get; init; }

    [Key((double)23.0)]
    public required IInterface DependencyDouble { get; init; }

    [Key((decimal)24)]
    public required IInterface DependencyDecimal { get; init; }

    [Key("25")]
    public required IInterface DependencyString { get; init; }

    [Key(false)]
    public required IInterface DependencyBool { get; init; }*/
}

[CreateFunction(typeof(Root), "Create")]
internal partial class Container
{
    private Container() {}
}