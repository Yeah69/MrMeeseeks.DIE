using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Func.OverrideCombination;

internal sealed class Dependency
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

internal sealed class Parent0
{
    public Dependency Dependency { get; }
    
    // Overrides in DIE are only applied until the next Func-injection
    // So if the previous int-override is required in the next Func-injection, it has to be manually passed
    internal Parent0(
        int valueInt,
        Func<int, string, Dependency> fac) =>
        Dependency = fac(valueInt, "1");
}

internal sealed class Parent1
{
    public Dependency Dependency { get; }
    
    internal Parent1(
        Func<int, Parent0> fac) =>
        Dependency = fac(2).Dependency;
}

[CreateFunction(typeof(Parent1), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var parent = container.Create();
        Assert.IsType<Parent1>(parent);
        Assert.Equal(2, parent.Dependency.ValueInt);
        Assert.Equal("1", parent.Dependency.ValueString);
    }
}
