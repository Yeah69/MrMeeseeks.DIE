using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Func.OverrideCombination;

internal class Dependency
{
    public int ValueInt { get; }
    public string ValueString { get; }

    internal Dependency(
        int valueInt,
        string valueString)
    {
        ValueInt = valueInt;
        ValueString = valueString;
    }
}

internal class Parent0
{
    public Dependency Dependency { get; }
    
    internal Parent0(
        Func<string, Dependency> fac) =>
        Dependency = fac("1");
}

internal class Parent1
{
    public Dependency Dependency { get; }
    
    internal Parent1(
        Func<int, Parent0> fac) =>
        Dependency = fac(2).Dependency;
}

[CreateFunction(typeof(Parent1), "Create")]
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
        Assert.IsType<Parent1>(parent);
        Assert.Equal(2, parent.Dependency.ValueInt);
        Assert.Equal("1", parent.Dependency.ValueString);
    }
}
