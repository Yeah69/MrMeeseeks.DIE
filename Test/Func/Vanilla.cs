using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public async ValueTask Test()
    {
        await using var container = new Container();
        var _ = container.Create()(DateTime.Now, new List<object>());
    }
}
