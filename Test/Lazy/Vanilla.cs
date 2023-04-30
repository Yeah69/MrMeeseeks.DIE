using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Lazy.Vanilla;

internal class Dependency{}

[CreateFunction(typeof(Lazy<Dependency>), "Create")]
internal sealed partial class Container
{
    private Container() {}
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var lazy = container.Create();
        var _ = lazy.Value;
    }
}
