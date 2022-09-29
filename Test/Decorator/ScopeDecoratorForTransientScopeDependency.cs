using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

namespace MrMeeseeks.DIE.Test.Decorator.ScopeDecoratorForTransientScopeDependency;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal class Dependency : IInterface, ITransientScopeInstance
{
    public IInterface Decorated => this;
}

internal class Decorator : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated { get; }
    
    internal Decorator(IInterface decorated) => Decorated = decorated;
}

internal class ScopeRoot : IScopeRoot
{
    public IInterface Decorated { get; }

    internal ScopeRoot(IInterface decorated) => Decorated = decorated;
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container
{
}

public class Tests
{
    [Fact]
    public void Test()
    {
        using var container = new Container();

        var scopeRoot0 = container.Create();
        var decorator0 = scopeRoot0.Decorated;
        var dependency0 = decorator0.Decorated;
        
        var scopeRoot1 = container.Create();
        var decorator1 = scopeRoot1.Decorated;
        var dependency1 = decorator1.Decorated;
        
        Assert.IsType<Decorator>(decorator0);
        Assert.IsType<Decorator>(decorator1);
        Assert.IsType<Dependency>(dependency0);
        Assert.IsType<Dependency>(dependency1);
        
        Assert.NotSame(decorator0, decorator1);
        Assert.Same(dependency0, dependency1);
    }
}