using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Func.OverrideScoped;

internal sealed class DependencyInner
{
    internal DependencyInner(
        // ReSharper disable once UnusedParameter.Local
        Lazy<Parent> lazyParent,
        // ReSharper disable once UnusedParameter.Local
        string anotherDependency)
    {
        
    }
}

internal sealed class Dependency
{
    public int Value { get; }

    // ReSharper disable once UnusedParameter.Local
    internal Dependency(int value, Func<string, DependencyInner> fac) => Value = value;
}

internal sealed class Parent : IScopeRoot
{
    public Dependency Dependency { get; }
    
    internal Parent(
        Func<int, Dependency> fac) =>
        Dependency = fac(1);
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedMember.Local
    private int DIE_Factory_int => 0;
    
    
}

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var parent = container.Create();
        Assert.IsType<Parent>(parent);
        Assert.Equal(1, parent.Dependency.Value);
    }
}
