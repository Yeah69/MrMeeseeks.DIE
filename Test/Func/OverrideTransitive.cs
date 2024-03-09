using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Func.OverrideTransitive;

internal class Dependency
{
    public int ValueInt { get; }

    internal Dependency(
        int valueInt)
    {
        ValueInt = valueInt;
    }
}

internal class Parent0 : ITransientScopeRoot
{
    public Dependency Dependency { get; }
    
    internal Parent0(
        Dependency dependency) =>
        Dependency = dependency;
}

internal class Parent1
{
    public Dependency Dependency { get; }
    
    internal Parent1(
        Func<int, Parent0> fac) =>
        Dependency = fac(2).Dependency;
}

[CreateFunction(typeof(Parent1), "Create")]
internal sealed partial class Container;

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var parent = container.Create();
        Assert.IsType<Parent1>(parent);
        Assert.Equal(2, parent.Dependency.ValueInt);
    }
}
