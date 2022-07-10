using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Lazy.Override;

internal class Dependency
{
    internal int Value { get; }
    internal Dependency(int value) => Value = value;
}

internal class Parent0
{
    internal Dependency Dependency { get; }
    internal Parent0(Lazy<Dependency> dependency) => Dependency = dependency.Value;
}

internal class Parent1
{
    internal Dependency Dependency { get; }
    internal Parent1(Func<int, Parent0> fac) => Dependency = fac(23).Dependency;
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
        Assert.Equal(23, parent.Dependency.Value);
    }
}
