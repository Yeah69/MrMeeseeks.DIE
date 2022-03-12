using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Sample;

namespace MrMeeseeks.DIE.Test;

internal interface IDecoratedScopeRoot
{
    IDecoratedScopeRootDependency Dependency { get; }
    IDecoratedScopeRoot Decorated { get; }
}

internal interface IDecoratedScopeRootDependency {}

internal class DecoratedScopeRootDependency : IDecoratedScopeRootDependency, IScopeInstance {}

internal class DecoratorScopeRootBasis : IDecoratedScopeRoot, IScopeRoot, IScopeInstance
{
    public IDecoratedScopeRootDependency Dependency { get; }

    public IDecoratedScopeRoot Decorated => this;

    public DecoratorScopeRootBasis(
        IDecoratedScopeRootDependency dependency) =>
        Dependency = dependency;
}

internal class DecoratorScopeRoot : IDecoratedScopeRoot, IDecorator<IDecoratedScopeRoot>
{
    public DecoratorScopeRoot(IDecoratedScopeRoot decorated, IDecoratedScopeRootDependency dependency)
    {
        Decorated = decorated;
        Dependency = dependency;
    }

    public IDecoratedScopeRootDependency Dependency { get; }
    public IDecoratedScopeRoot Decorated { get; }
}

[MultiContainer(typeof(IDecoratedScopeRoot))]
internal partial class DecoratorScopeRootContainer
{
    
}