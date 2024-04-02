using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Func.OverrideExistingOverride;

internal sealed class Dependency
{
    public int Value { get; }

    internal Dependency(int value) => Value = value;
}

internal sealed class Parent0
{
    public Dependency Dependency { get; }
    
    internal Parent0(
        Func<int, Dependency> fac) =>
        Dependency = fac(6);
}

internal sealed class Parent1
{
    public Dependency Dependency { get; }
    
    internal Parent1(
        Func<int, Parent0> fac) =>
        Dependency = fac(1).Dependency;
}

[CreateFunction(typeof(Parent1), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var parent = container.Create();
        Assert.IsType<Parent1>(parent);
        Assert.Equal(6, parent.Dependency.Value);
    }
}
