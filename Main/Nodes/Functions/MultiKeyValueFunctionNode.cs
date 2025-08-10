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

internal interface IMultiKeyValueFunctionNode : IMultiFunctionNodeBase;

internal sealed partial class MultiKeyValueFunctionNode : MultiFunctionNodeBase, IMultiKeyValueFunctionNode, IScopeInstance
{
    private readonly INamedTypeSymbol _enumerableType;
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly Func<INamedTypeSymbol, object, IElementNode, IKeyValuePairNode> _keyValuePairNodeFactory;
    private readonly TypeSymbolUtility _typeSymbolUtility;
    private readonly ICheckTypeProperties _checkTypeProperties;

    internal MultiKeyValueFunctionNode(
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
        AsynchronicityHandlingFactory asynchronicityHandlingFactory,
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
        TypeSymbolUtility typeSymbolUtility,
        ICheckTypeProperties checkTypeProperties,
        WellKnownTypesCollections wellKnownTypesCollections)
        : base(
            enumerableType, 
            parameters, 
            parentContainer, 
            subDisposalNodeChooser,
            transientScopeDisposalNodeChooser,
            asynchronicityHandlingFactory,
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
            wellKnownTypesCollections)
    {
        _enumerableType = enumerableType;
        _localDiagLogger = localDiagLogger;
        _keyValuePairNodeFactory = keyValuePairNodeFactory;
        _typeSymbolUtility = typeSymbolUtility;
        _checkTypeProperties = checkTypeProperties;

        NamePrefix = $"CreateMultiKeyValue{_enumerableType.Name}";
        NameNumberSuffix = referenceGenerator.Generate("");
    }

    private static IElementNode MapToReturnedElement(IElementNodeMapperBase mapper, ITypeSymbol itemType) =>
        mapper.Map(itemType, new(ImmutableStack<INamedTypeSymbol>.Empty, null));
    
    public override void Build(PassedContext passedContext)
    {
        base.Build(passedContext);
        var keyValueType = (INamedTypeSymbol) _enumerableType.TypeArguments[0];
        var itemType = keyValueType.TypeArguments[1];
        if (_typeSymbolUtility.GetUnwrappedType(itemType) is not INamedTypeSymbol unwrappedItemType)
        {
            _localDiagLogger.Error(ErrorLogData.ResolutionException("The value type of the keyed map is non-iterable, therefore it has to be a named type (class, struct or interface).", _enumerableType, ImmutableStack<INamedTypeSymbol>.Empty), Location.None);
            throw new InvalidOperationException();
        }
        var concreteItemTypesMap = _checkTypeProperties.MapToKeyedImplementations(unwrappedItemType, keyValueType.TypeArguments[0]);

        ReturnedElements = concreteItemTypesMap
            .Select(kvp => _keyValuePairNodeFactory(
                    keyValueType,
                    kvp.Key,
                    MapToReturnedElement(
                        GetMapper(unwrappedItemType, kvp.Value),
                        itemType)))
            .ToList();
    }

    protected override string NamePrefix { get; set; }
    protected override string NameNumberSuffix { get; set; }
}