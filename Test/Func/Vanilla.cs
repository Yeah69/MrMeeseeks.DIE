using System;
using System.Collections.Generic;
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
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var _ = container.Create()(DateTime.Now, new List<object>());
    }
}
