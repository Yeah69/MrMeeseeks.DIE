using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Func.OverrideMultipleParameterOfSameType;

internal class Dependency
{
    public int Value { get; }

    internal Dependency(int value) => Value = value;
}

internal class Parent
{
    public Dependency Dependency { get; }
    
    internal Parent(
        Func<int, int, int, Dependency> fac) =>
        Dependency = fac(1, 2, 3);
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container
{
    private int DIE_Factory_int => 0;
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var parent = container.Create();
        Assert.IsType<Parent>(parent);
        Assert.Equal(1, parent.Dependency.Value);
    }
}
