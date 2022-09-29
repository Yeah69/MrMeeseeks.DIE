using System;
using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Func.Vanilla;

internal class Dependency{}

[CreateFunction(typeof(Func<DateTime, IList<object>, Dependency>), "Create")]
internal sealed partial class Container
{
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var _ = container.Create()(DateTime.Now, new List<object>());
    }
}
