using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ISingleFunctionNode : IFunctionNode
{
    IElementNode ReturnedElement { get; }
}

internal abstract class SingleFunctionNodeBase : ReturningFunctionNodeBase, ISingleFunctionNode
{
    public SingleFunctionNodeBase(
        // parameters
        Accessibility? accessibility,
        ITypeSymbol typeSymbol,
        IReadOnlyList<ITypeSymbol> parameters,
        ImmutableDictionary<ITypeSymbol, IParameterNode> closureParameters,
        IRangeNode parentRange,
        IContainerNode parentContainer,
        
        // dependencies
        ISubDisposalNodeChooser subDisposalNodeChooser,
        ITransientScopeDisposalNodeChooser transientScopeDisposalNodeChooser,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        Func<PlainFunctionCallNode.Params, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<WrappedAsyncFunctionCallNode.Params, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ScopeCallNode.Params, IScopeCallNode> scopeCallNodeFactory,
        Func<TransientScopeCallNode.Params, ITransientScopeCallNode> transientScopeCallNodeFactory,
        ITypeParameterUtility typeParameterUtility,
        WellKnownTypes wellKnownTypes)
        : base(
            accessibility, 
            typeSymbol, 
            parameters, 
            closureParameters, 
            parentContainer, 
            parentRange,
            subDisposalNodeChooser,
            transientScopeDisposalNodeChooser,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            asyncFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            typeParameterUtility,
            wellKnownTypes)
    {
        ReturnedTypeNameNotWrapped = typeSymbol.Name;
    }

    protected abstract IElementNodeMapperBase GetMapper();

    protected virtual IElementNode MapToReturnedElement(IElementNodeMapperBase mapper) =>
        mapper.Map(TypeSymbol, new(ImmutableStack<INamedTypeSymbol>.Empty, null));
    
    public override void Build(PassedContext passedContext)
    {
        base.Build(passedContext);
        ReturnedElement = MapToReturnedElement(GetMapper());
    }

    public IElementNode ReturnedElement { get; private set; } = null!;
    public override string ReturnedTypeNameNotWrapped { get; }
}