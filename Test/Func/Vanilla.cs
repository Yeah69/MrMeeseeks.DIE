using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Func.Vanilla;

internal sealed class Dependency;

[CreateFunction(typeof(Func<DateTime, IList<object>, Dependency>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        _ = container.Create()(DateTime.Now, new List<object>());
    }
}
