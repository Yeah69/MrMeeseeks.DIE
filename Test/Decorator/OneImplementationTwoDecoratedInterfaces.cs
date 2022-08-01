using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Decorator.OneImplementationTwoDecoratedInterfaces;

internal interface IInterfaceA
{
    IInterfaceA DecoratedA { get; }
}

internal interface IInterfaceB
{
    IInterfaceB DecoratedB { get; }
}

internal class Dependency : IInterfaceA, IInterfaceB
{
    public IInterfaceA DecoratedA => this;
    public IInterfaceB DecoratedB => this;
}

internal class DecoratorAA : IInterfaceA, IDecorator<IInterfaceA>
{
    public IInterfaceA DecoratedA { get; }
    
    internal DecoratorAA(IInterfaceA decorated) => DecoratedA = decorated;
}

internal class DecoratorAB : IInterfaceA, IDecorator<IInterfaceA>
{
    public IInterfaceA DecoratedA { get; }
    
    internal DecoratorAB(IInterfaceA decorated) => DecoratedA = decorated;
}

internal class DecoratorBA : IInterfaceB, IDecorator<IInterfaceB>
{
    public IInterfaceB DecoratedB { get; }
    
    internal DecoratorBA(IInterfaceB decorated) => DecoratedB = decorated;
}

internal class DecoratorBB : IInterfaceB, IDecorator<IInterfaceB>
{
    public IInterfaceB DecoratedB { get; }
    
    internal DecoratorBB(IInterfaceB decorated) => DecoratedB = decorated;
}

internal class Parent
{
    public IInterfaceA DependencyA { get; }
    public IInterfaceB DependencyB { get; }

    internal Parent(
        IInterfaceA dependencyA,
        IInterfaceB dependencyB)
    {
        DependencyA = dependencyA;
        DependencyB = dependencyB;
    }
}


[CreateFunction(typeof(Parent), "Create")]
[DecoratorSequenceChoice(typeof(IInterfaceA), typeof(Dependency), typeof(DecoratorAA), typeof(DecoratorAB))]
[DecoratorSequenceChoice(typeof(IInterfaceB), typeof(Dependency), typeof(DecoratorBA), typeof(DecoratorBB))]
internal sealed partial class Container
{
}

public class Tests
{
    [Fact]
    public async ValueTask Test()
    {
        await using var container = new Container();
        var parent = container.Create();
        Assert.IsType<DecoratorAB>(parent.DependencyA.DecoratedA);
        Assert.IsType<DecoratorAA>(parent.DependencyA.DecoratedA.DecoratedA);
        Assert.IsType<Dependency>(parent.DependencyA.DecoratedA.DecoratedA.DecoratedA);
        Assert.IsType<DecoratorBB>(parent.DependencyB.DecoratedB);
        Assert.IsType<DecoratorBA>(parent.DependencyB.DecoratedB.DecoratedB);
        Assert.IsType<Dependency>(parent.DependencyB.DecoratedB.DecoratedB.DecoratedB);
    }
}