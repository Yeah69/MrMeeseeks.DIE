using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ISubDisposalNodeChooser
{
    IElementNode ChooseSubDisposalNode();
}

internal interface IOuterFunctionSubDisposalNodeChooser : ISubDisposalNodeChooser;

internal sealed class OuterFunctionSubDisposalNodeChooser : IOuterFunctionSubDisposalNodeChooser
{
    private readonly IContainerNode _parentContainer;
    private readonly Func<IInitialOrdinarySubDisposalNode> _initialSubDisposalNodeFactory;

    internal OuterFunctionSubDisposalNodeChooser(
        IContainerNode parentContainer,
        Func<IInitialOrdinarySubDisposalNode> initialSubDisposalNodeFactory)
    {
        _parentContainer = parentContainer;
        _initialSubDisposalNodeFactory = initialSubDisposalNodeFactory;
    }
    public IElementNode ChooseSubDisposalNode() => 
        _initialSubDisposalNodeFactory().EnqueueBuildJobTo(_parentContainer.BuildQueue, PassedContext.Empty);
}

internal interface IInnerFunctionSubDisposalNodeChooser : ISubDisposalNodeChooser;

internal sealed class InnerFunctionSubDisposalNodeChooser : IInnerFunctionSubDisposalNodeChooser
{
    private readonly IContainerNode _parentContainer;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Func<ITypeSymbol, IParameterNode> _parameterNodeFactory;

    internal InnerFunctionSubDisposalNodeChooser(
        IContainerNode parentContainer,
        WellKnownTypes wellKnownTypes,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory)
    {
        _parentContainer = parentContainer;
        _wellKnownTypes = wellKnownTypes;
        _parameterNodeFactory = parameterNodeFactory;
    }

    public IElementNode ChooseSubDisposalNode() =>
        _parameterNodeFactory(_wellKnownTypes.ConcurrentStackOfObject).EnqueueBuildJobTo(_parentContainer.BuildQueue, PassedContext.Empty);
}