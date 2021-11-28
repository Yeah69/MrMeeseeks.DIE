namespace MrMeeseeks.DIE.Sample;

internal interface IDecoratedScopeRoot
{
    IDecoratedScopeRoot Decorated { get; }
}

internal class DecoratorScopeRootBasis : IDecoratedScopeRoot, IScopeRoot
{
    public IDecoratedScopeRoot Decorated => this;
}

internal class DecoratorScopeRoot : IDecoratedScopeRoot, IDecorator<IDecoratedScopeRoot>
{
    public DecoratorScopeRoot(IDecoratedScopeRoot decorated) => 
        Decorated = decorated;

    public IDecoratedScopeRoot Decorated { get; }
}

internal partial class DecoratorScopeRootContainer : IContainer<IDecoratedScopeRoot>
{
    
}