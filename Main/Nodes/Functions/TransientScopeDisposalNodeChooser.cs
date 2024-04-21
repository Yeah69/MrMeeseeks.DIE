using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ITransientScopeDisposalNodeChooser
{
    IElementNode ChooseTransientScopeDisposalNode();
}

internal interface IEntryTransientScopeDisposalNodeChooser : ITransientScopeDisposalNodeChooser;

internal sealed class EntryTransientScopeDisposalNodeChooser : IEntryTransientScopeDisposalNodeChooser
{
    private readonly IContainerNode _parentContainer;
    private readonly Func<IInitialTransientScopeSubDisposalNode> _initialSubDisposalNodeFactory;

    internal EntryTransientScopeDisposalNodeChooser(
        IContainerNode parentContainer,
        Func<IInitialTransientScopeSubDisposalNode> initialSubDisposalNodeFactory)
    {
        _parentContainer = parentContainer;
        _initialSubDisposalNodeFactory = initialSubDisposalNodeFactory;
    }
    public IElementNode ChooseTransientScopeDisposalNode() => 
        _initialSubDisposalNodeFactory().EnqueueBuildJobTo(_parentContainer.BuildQueue, PassedContext.Empty);
}

internal interface IInnerTransientScopeDisposalNodeChooser : ITransientScopeDisposalNodeChooser;

internal sealed class InnerTransientScopeDisposalNodeChooser : IInnerTransientScopeDisposalNodeChooser
{
    private readonly IContainerNode _parentContainer;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Func<ITypeSymbol, IParameterNode> _parameterNodeFactory;

    internal InnerTransientScopeDisposalNodeChooser(
        IContainerNode parentContainer,
        WellKnownTypes wellKnownTypes,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory)
    {
        _parentContainer = parentContainer;
        _wellKnownTypes = wellKnownTypes;
        _parameterNodeFactory = parameterNodeFactory;
    }

    public IElementNode ChooseTransientScopeDisposalNode() =>
        _parameterNodeFactory(_wellKnownTypes.ListOfObject).EnqueueBuildJobTo(_parentContainer.BuildQueue, PassedContext.Empty);
}