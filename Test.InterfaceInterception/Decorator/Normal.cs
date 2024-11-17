using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.InterfaceInterception.Decorator.Normal;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal sealed class Dependency : IInterface
{
    public IInterface Decorated => this;
}

internal sealed class Decorator : IInterface, IDecorator<IInterface>
{
    public Decorator(IInterface decoratedNormal) => 
        Decorated = decoratedNormal;

    public IInterface Decorated { get; }
}

[CreateFunction(typeof(IInterface), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var decorated = container.Create();
        Assert.NotEqual(decorated, decorated.Decorated);
        Assert.IsType<Decorator>(decorated);
        Assert.IsType<Dependency>(decorated.Decorated);
    }
}