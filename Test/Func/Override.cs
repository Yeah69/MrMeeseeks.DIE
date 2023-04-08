using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Func.Override;

internal class Dependency
{
    public int Value { get; }

    internal Dependency(int value) => Value = value;
}

internal class Parent
{
    public Dependency Dependency { get; }
    
    internal Parent(
        Func<int, Dependency> fac) =>
        Dependency = fac(1);
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container
{
    private int DIE_Factory_int => 0;
    
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
        Assert.Equal(1, parent.Dependency.Value);
    }
}
