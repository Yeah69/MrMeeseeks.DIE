using System;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Lazy.Vanilla;

internal class Dependency{}

[MultiContainer(typeof(Lazy<Dependency>))]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var lazy = container.Create0();
        var _ = lazy.Value;
    }
}
