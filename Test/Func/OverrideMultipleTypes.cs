using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Func.OverrideMultipleTypes;

internal class Dependency
{
    public int ValueInt { get; }
    public uint ValueUint { get; }
    public long ValueLong { get; }
    public ulong ValueUlong { get; }
    public string ValueString { get; }

    internal Dependency(
        int valueInt,
        uint valueUint,
        long valueLong,
        ulong valueUlong,
        string valueString)
    {
        ValueInt = valueInt;
        ValueUint = valueUint;
        ValueLong = valueLong;
        ValueUlong = valueUlong;
        ValueString = valueString;
    }
}

internal class Parent
{
    public Dependency Dependency { get; }
    
    internal Parent(
        Func<ulong, uint, string, int, long, Dependency> fac) =>
        Dependency = fac(0, 1, "2", 3, 4);
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var parent = container.Create();
        Assert.IsType<Parent>(parent);
        Assert.IsType<Dependency>(parent.Dependency);
    }
}