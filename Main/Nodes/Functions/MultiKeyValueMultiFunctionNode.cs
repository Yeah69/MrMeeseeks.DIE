using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IMultiKeyValueMultiFunctionNode : IMultiFunctionNodeBase
{
}

internal partial class MultiKeyValueMultiFunctionNode : MultiFunctionNodeBase, IMultiKeyValueMultiFunctionNode, IScopeInstance
{
    private readonly INamedTypeSymbol _enumerableType;
    private readonly Func<IElementNodeMapper> _typeToElementNodeMapperFactory;
    private readonly Func<INamedTypeSymbol, object, IElementNode, IKeyValuePairNode> _keyValuePairNodeFactory;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly WellKnownTypes _wellKnownTypes;

    internal MultiKeyValueMultiFunctionNode(
        // parameters
        INamedTypeSymbol enumerableType,
        IReadOnlyList<ITypeSymbol> parameters,
        
        // dependencies
        IContainerNode parentContainer,
        ITransientScopeWideContext transientScopeWideContext,
        IReferenceGenerator referenceGenerator,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        Func<string?, IReadOnlyList<(IParameterNode, IParameterNode)>, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<ITypeSymbol, string?, SynchronicityDecision, IReadOnlyList<(IParameterNode, IParameterNode)>, IAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<(string, string), IScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<IElementNodeMapper> typeToElementNodeMapperFactory,
        Func<IElementNodeMapperBase, (INamedTypeSymbol, INamedTypeSymbol), IOverridingElementNodeWithDecorationMapper> overridingElementNodeWithDecorationMapperFactory,
        Func<INamedTypeSymbol, object, IElementNode, IKeyValuePairNode> keyValuePairNodeFactory,
        IContainerWideContext containerWideContext)
        : base(
            enumerableType, 
            parameters, 
            parentContainer, 
            transientScopeWideContext,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            asyncFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            typeToElementNodeMapperFactory,
            overridingElementNodeWithDecorationMapperFactory,
            containerWideContext)
    {
        _enumerableType = enumerableType;
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        _keyValuePairNodeFactory = keyValuePairNodeFactory;
        _checkTypeProperties = transientScopeWideContext.CheckTypeProperties;
        _wellKnownTypes = containerWideContext.WellKnownTypes;

        Name = referenceGenerator.Generate("CreateMultiKeyValue", enumerableType);
    }

    private IElementNode MapToReturnedElement(IElementNodeMapperBase mapper, ITypeSymbol itemType, ITypeSymbol keyType, object keyValue) =>
        mapper.Map(itemType, new(ImmutableStack<INamedTypeSymbol>.Empty, new(keyType, keyValue)));
    
    public override void Build(PassedContext passedContext)
    {
        base.Build(passedContext);
        var keyValueType = (INamedTypeSymbol) _enumerableType.TypeArguments[0];
        var itemType = keyValueType.TypeArguments[1];
        var unwrappedItemType = TypeSymbolUtility.GetUnwrappedType(itemType, _wellKnownTypes) as INamedTypeSymbol ?? throw new InvalidOperationException(); // todo replace exception with error log
        var iterableItemType = CollectionUtility.GetCollectionsInnerType(unwrappedItemType) as INamedTypeSymbol ?? throw new InvalidOperationException(); // todo replace exception with error log
        
        var concreteItemTypesMap = _checkTypeProperties.MapToKeyedMultipleImplementations(iterableItemType, keyValueType.TypeArguments[0]);

        ReturnedElements = concreteItemTypesMap
            .Select(kvp => _keyValuePairNodeFactory(
                    keyValueType,
                    kvp.Key,
                    MapToReturnedElement(
                        _typeToElementNodeMapperFactory(),
                        itemType,
                        keyValueType.TypeArguments[0],
                        kvp.Key)))
            .ToList();
    }

    public override string Name { get; protected set; }
}