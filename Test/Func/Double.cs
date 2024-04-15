using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Func.Double;

internal sealed class Dependency;

internal sealed class Parent
{
    internal Parent(
        // ReSharper disable once UnusedParameter.Local
        Func<Dependency> fac0,
        // ReSharper disable once UnusedParameter.Local
        Func<Dependency> fac1)
    {
        
    }
}

[CreateFunction(typeof(Parent), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var parent = container.Create();
        Assert.IsType<Parent>(parent);
    }
}
