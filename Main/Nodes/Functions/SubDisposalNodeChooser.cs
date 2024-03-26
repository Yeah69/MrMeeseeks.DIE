using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ISubDisposalNodeChooser
{
    IElementNode ChooseSubDisposalNode(PassedContext passedContext);
}

internal interface IOuterFunctionSubDisposalNodeChooser : ISubDisposalNodeChooser;

internal sealed class OuterFunctionSubDisposalNodeChooser : IOuterFunctionSubDisposalNodeChooser
{
    private readonly IContainerNode _parentContainer;
    private readonly Func<IInitialSubDisposalNode> _initialSubDisposalNodeFactory;

    internal OuterFunctionSubDisposalNodeChooser(
        IContainerNode parentContainer,
        Func<IInitialSubDisposalNode> initialSubDisposalNodeFactory)
    {
        _parentContainer = parentContainer;
        _initialSubDisposalNodeFactory = initialSubDisposalNodeFactory;
    }
    public IElementNode ChooseSubDisposalNode(PassedContext passedContext) => 
        _initialSubDisposalNodeFactory().EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
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

    public IElementNode ChooseSubDisposalNode(PassedContext passedContext) =>
        _parameterNodeFactory(_wellKnownTypes.ListOfObject).EnqueueBuildJobTo(_parentContainer.BuildQueue, passedContext);
}