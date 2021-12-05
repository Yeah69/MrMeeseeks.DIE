namespace MrMeeseeks.DIE.Sample;

internal interface IDecoratedScopeRoot
{
    IDecoratedScopeRootDependency Dependency { get; }
    IDecoratedScopeRoot Decorated { get; }
}

internal interface IDecoratedScopeRootDependency {}

internal class DecoratedScopeRootDependency : IDecoratedScopeRootDependency, IScopedInstance { }

internal class DecoratorScopeRootBasis : IDecoratedScopeRoot, IScopeRoot, IScopedInstance
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

internal partial class DecoratorScopeRootContainer : IContainer<IDecoratedScopeRoot>
{
    
}