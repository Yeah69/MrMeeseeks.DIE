using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ISingleFunctionNode : IFunctionNode
{
    IElementNode ReturnedElement { get; }
}

internal abstract class SingleFunctionNodeBase : FunctionNodeBase, ISingleFunctionNode
{
    private readonly ITypeSymbol _typeSymbol;
    private readonly IRangeNode _parentNode;
    private readonly IContainerNode _parentContainer;
    private readonly IUserDefinedElements _userDefinedElements;
    private readonly ICheckTypeProperties _checkTypeProperties;

    public SingleFunctionNodeBase(
        // parameters
        Accessibility? accessibility,
        ITypeSymbol typeSymbol,
        IReadOnlyList<ITypeSymbol> parameters,
        ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)> closureParameters,
        IRangeNode parentNode,
        IContainerNode parentContainer,
        IUserDefinedElements userDefinedElements,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        
        // dependencies
        Func<ITypeSymbol, IReferenceGenerator, IParameterNode> parameterNodeFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<string, string, IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IContainerNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, ITransientScopeCallNode> transientScopeCallNodeFactory,
        WellKnownTypes wellKnownTypes)
        : base(
            accessibility, 
            typeSymbol, 
            parameters, 
            closureParameters, 
            parentContainer, 
            parentNode,
            referenceGenerator,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            wellKnownTypes)
    {
        _typeSymbol = typeSymbol;
        _parentNode = parentNode;
        _parentContainer = parentContainer;
        _userDefinedElements = userDefinedElements;
        _checkTypeProperties = checkTypeProperties;
    }

    protected abstract IElementNodeMapperBase GetMapper(
        ISingleFunctionNode parentFunction,
        IRangeNode parentNode,
        IContainerNode parentContainer,
        IUserDefinedElements userDefinedElements,
        ICheckTypeProperties checkTypeProperties);

    protected virtual IElementNode MapToReturnedElement(IElementNodeMapperBase mapper) =>
        mapper.Map(_typeSymbol, ImmutableStack.Create<INamedTypeSymbol>());
    
    public override void Build(ImmutableStack<INamedTypeSymbol> implementationStack) => ReturnedElement = MapToReturnedElement(
        GetMapper(this, _parentNode, _parentContainer, _userDefinedElements, _checkTypeProperties));
    public IElementNode ReturnedElement { get; private set; } = null!;
}