using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

namespace MrMeeseeks.DIE.Test.InterfaceInterception.Decorator.Ordinal.NotConfigured;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal sealed class DecoratorUh : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

internal sealed class DecoratorY : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

internal sealed class DecoratorE : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

internal sealed class DecoratorA : IInterface, IDecorator<IInterface>
{
    public required IInterface Decorated { get; internal init; }
}

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

        var set = new HashSet<Type>(new[]
        {
            typeof(DecoratorUh),
            typeof(DecoratorY),
            typeof(DecoratorE),
            typeof(DecoratorA),
            typeof(DecoratorH),
        });
        
        var current = container.Create();
        for (var i = 0; i < 5; i++)
        {
            Assert.True(set.Remove(current.GetType()));
            current = current.Decorated;
        }
        Assert.IsType<Dependency>(current);
    }
}