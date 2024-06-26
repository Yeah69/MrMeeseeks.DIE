using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Decorator.ScopeDecoratorForTransientScopeDependency;

internal interface IInterface
{
    IInterface Decorated { get; }
}

internal sealed class Dependency : IInterface, ITransientScopeInstance
{
    public IInterface Decorated => this;
}

internal sealed class Decorator : IInterface, IDecorator<IInterface>
{
    public IInterface Decorated { get; }
    
    internal Decorator(IInterface decorated) => Decorated = decorated;
}

internal sealed class ScopeRoot : IScopeRoot
{
    public IInterface Decorated { get; }

    internal ScopeRoot(IInterface decorated) => Decorated = decorated;
}

[CreateFunction(typeof(ScopeRoot), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();

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