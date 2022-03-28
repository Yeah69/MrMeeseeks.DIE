using System;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Lazy.Vanilla;

internal class Dependency{}

[CreateFunction(typeof(Lazy<Dependency>), "CreateDep")]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var lazy = container.CreateDep();
        var _ = lazy.Value;
    }
}