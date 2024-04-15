using System;
using System.Threading;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.ThreadLocal.Override;

internal sealed class Dependency
{
    internal int Value { get; }
    internal Dependency(int value) => Value = value;
}

internal sealed class Parent0
{
    internal Dependency? Dependency { get; }
    internal Parent0(ThreadLocal<Dependency> dependency) => Dependency = dependency.Value;
}

internal sealed class Parent1
{
    internal Dependency? Dependency { get; }
    internal Parent1(Func<int, Parent0> fac) => Dependency = fac(23).Dependency;
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
        Assert.Equal(23, parent.Dependency?.Value);
    }
}
