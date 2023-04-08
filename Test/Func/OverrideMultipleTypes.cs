using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
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
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var parent = container.Create();
        Assert.IsType<Parent>(parent);
        Assert.IsType<Dependency>(parent.Dependency);
    }
}
