﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.Decorator.Ordinal.Choice;

internal interface IInterface
{
    IInterface Decorated { get; }
}

[DecorationOrdinal(69)]
internal sealed class DecoratorUh : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

[DecorationOrdinal(23)]
internal sealed class DecoratorY : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

[DecorationOrdinal(3)]
internal sealed class DecoratorE : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

internal sealed class DecoratorA : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

[DecorationOrdinal(-1)]
internal sealed class DecoratorH : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

internal sealed class Dependency : IInterface
{
    public IInterface Decorated => this;
}

[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var uh = container.Create();
        var y = uh.Decorated;
        var e = y.Decorated;
        var a = e.Decorated;
        var h = a.Decorated;
        var implementation = h.Decorated;
        
        Assert.IsType<DecoratorUh>(uh);
        Assert.IsType<DecoratorY>(y);
        Assert.IsType<DecoratorE>(e);
        Assert.IsType<DecoratorA>(a);
        Assert.IsType<DecoratorH>(h);
        Assert.IsType<Dependency>(implementation);
    }
}