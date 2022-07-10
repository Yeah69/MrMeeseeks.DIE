using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Func.OverrideExistingOverride;

internal class Dependency
{
    public int Value { get; }

    internal Dependency(int value) => Value = value;
}

internal class Parent0
{
    public Dependency Dependency { get; }
    
    internal Parent0(
        Func<int, Dependency> fac) =>
        Dependency = fac(6);
}

internal class Parent1
{
    public Dependency Dependency { get; }
    
    internal Parent1(
        Func<int, Parent0> fac) =>
        Dependency = fac(1).Dependency;
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
        Assert.Equal(6, parent.Dependency.Value);
    }
}
