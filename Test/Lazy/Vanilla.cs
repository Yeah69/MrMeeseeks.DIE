using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Lazy.Vanilla;

internal class Dependency{}

[CreateFunction(typeof(Lazy<Dependency>), "Create")]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public void Test()
    {
        var container = new Container();
        var lazy = container.Create();
        var _ = lazy.Value;
    }
}
