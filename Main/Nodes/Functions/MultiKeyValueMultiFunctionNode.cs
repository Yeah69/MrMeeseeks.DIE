using MrMeeseeks.DIE.CodeGeneration.Nodes;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IMultiKeyValueMultiFunctionNode : IMultiFunctionNodeBase;

internal sealed partial class MultiKeyValueMultiFunctionNode : MultiFunctionNodeBase, IMultiKeyValueMultiFunctionNode, IScopeInstance
{
    private readonly INamedTypeSymbol _enumerableType;
    private readonly ILocalDiagLogger _localDiagLogger;
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
        IRangeNode parentRange,
        IReferenceGenerator referenceGenerator,
        ILocalDiagLogger localDiagLogger,
        IInnerFunctionSubDisposalNodeChooser subDisposalNodeChooser,
        IInnerTransientScopeDisposalNodeChooser transientScopeDisposalNodeChooser,
        Lazy<IFunctionNodeGenerator> functionNodeGenerator,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        Func<PlainFunctionCallNode.Params, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<WrappedAsyncFunctionCallNode.Params, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ScopeCallNode.Params, IScopeCallNode> scopeCallNodeFactory,
        Func<TransientScopeCallNode.Params, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<IElementNodeMapper> typeToElementNodeMapperFactory,
        Func<IElementNodeMapperBase, (INamedTypeSymbol, INamedTypeSymbol), IOverridingElementNodeWithDecorationMapper> overridingElementNodeWithDecorationMapperFactory,
        Func<INamedTypeSymbol, object, IElementNode, IKeyValuePairNode> keyValuePairNodeFactory,
        ITypeParameterUtility typeParameterUtility,
        ICheckTypeProperties checkTypeProperties,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections)
        : base(
            enumerableType, 
            parameters, 
            parentContainer,
            subDisposalNodeChooser,
            transientScopeDisposalNodeChooser,
            functionNodeGenerator,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            asyncFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            typeToElementNodeMapperFactory,
            overridingElementNodeWithDecorationMapperFactory,
            typeParameterUtility,
            parentRange,
            wellKnownTypes,
            wellKnownTypesCollections)
    {
        _enumerableType = enumerableType;
        _localDiagLogger = localDiagLogger;
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        _keyValuePairNodeFactory = keyValuePairNodeFactory;
        _checkTypeProperties = checkTypeProperties;
        _wellKnownTypes = wellKnownTypes;

        Name = referenceGenerator.Generate("CreateMultiKeyValue", _enumerableType);
    }

    private static IElementNode MapToReturnedElement(IElementNodeMapperBase mapper, ITypeSymbol itemType, ITypeSymbol keyType, object keyValue) =>
        mapper.Map(itemType, new(ImmutableStack<INamedTypeSymbol>.Empty, new(keyType, keyValue)));
    
    public override void Build(PassedContext passedContext)
    {
        base.Build(passedContext);
        var keyValueType = (INamedTypeSymbol) _enumerableType.TypeArguments[0];
        var itemType = keyValueType.TypeArguments[1];
        var unwrappedItemType = TypeSymbolUtility.GetUnwrappedType(itemType, _wellKnownTypes);
        if (CollectionUtility.GetCollectionsInnerType(unwrappedItemType) is not INamedTypeSymbol iterableItemType)
        {
            _localDiagLogger.Error(ErrorLogData.ResolutionException("The iterable value type of the keyed map has to have a named item type (class, struct or interface).", _enumerableType, ImmutableStack<INamedTypeSymbol>.Empty), Location.None);
            throw new InvalidOperationException();
        }
        
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