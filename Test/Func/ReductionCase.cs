using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Func.ReductionCase;

internal class DependencyA
{
    public int Value { get; }
    public DependencyB Dependency { get; }

    internal DependencyA(
        int value,
        Func<int, DependencyB> dependencyB)
    {
        Value = value;
        Dependency = dependencyB(23);
    }
}


internal class DependencyB
{
    private readonly Lazy<Parent> _parentFactory;
    public int Value { get; }
    public Parent Parent => _parentFactory.Value;

    internal DependencyB(
        int value,
        Lazy<Parent> parentFactory)
    {
        _parentFactory = parentFactory;
        Value = value;
    }
}

internal class Parent
{
    public DependencyA Dependency { get; }
    
    internal Parent(
        DependencyA dependencyA) =>
        Dependency = dependencyA;
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
        Assert.True(0 == parent.Dependency.Value);
        Assert.True(23 == parent.Dependency.Dependency.Value);
        Assert.True(0 == parent.Dependency.Dependency.Parent.Dependency.Value);
    }
}
