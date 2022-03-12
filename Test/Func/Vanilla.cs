using System;
using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration;
using Xunit;

namespace MrMeeseeks.DIE.Test.Func.Vanilla;

internal class Dependency{}

[MultiContainer(typeof(Func<DateTime, IList<object>, Dependency>))]
internal partial class Container
{
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();
        var _ = container.Create0()(DateTime.Now, new List<object>());
    }
}
