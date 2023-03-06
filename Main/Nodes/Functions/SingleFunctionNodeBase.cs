using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ISingleFunctionNode : IFunctionNode
{
    IElementNode ReturnedElement { get; }
}

internal abstract class SingleFunctionNodeBase : ReturningFunctionNodeBase, ISingleFunctionNode
{
    private readonly IRangeNode _parentRange;
    private readonly IUserDefinedElementsBase _userDefinedElements;
    private readonly ICheckTypeProperties _checkTypeProperties;

    public SingleFunctionNodeBase(
        // parameters
        Accessibility? accessibility,
        ITypeSymbol typeSymbol,
        IReadOnlyList<ITypeSymbol> parameters,
        ImmutableDictionary<ITypeSymbol, IParameterNode> closureParameters,
        IRangeNode parentRange,
        IContainerNode parentContainer,
        IUserDefinedElementsBase userDefinedElements,
        ICheckTypeProperties checkTypeProperties,
        
        // dependencies
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<(string, string), IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        IContainerWideContext containerWideContext)
        : base(
            accessibility, 
            typeSymbol, 
            parameters, 
            closureParameters, 
            parentContainer, 
            parentRange,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            containerWideContext)
    {
        _parentRange = parentRange;
        _userDefinedElements = userDefinedElements;
        _checkTypeProperties = checkTypeProperties;
    }

    protected abstract IElementNodeMapperBase GetMapper(
        ISingleFunctionNode parentFunction,
        IRangeNode parentNode,
        IContainerNode parentContainer,
        IUserDefinedElementsBase userDefinedElements,
        ICheckTypeProperties checkTypeProperties);

    protected virtual IElementNode MapToReturnedElement(IElementNodeMapperBase mapper) =>
        mapper.Map(TypeSymbol, ImmutableStack.Create<INamedTypeSymbol>());
    
    public override void Build(ImmutableStack<INamedTypeSymbol> implementationStack) => ReturnedElement = MapToReturnedElement(
        GetMapper(this, _parentRange, ParentContainer, _userDefinedElements, _checkTypeProperties));
    public IElementNode ReturnedElement { get; private set; } = null!;
}