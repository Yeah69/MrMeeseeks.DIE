using MrMeeseeks.DIE.CodeGeneration.Nodes;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IMultiFunctionNode : IMultiFunctionNodeBase;

internal sealed partial class MultiFunctionNode : MultiFunctionNodeBase, IMultiFunctionNode, IScopeInstance
{
    private readonly INamedTypeSymbol _enumerableType;
    private readonly TypeSymbolUtility _typeSymbolUtility;
    private readonly ICheckTypeProperties _checkTypeProperties;

    internal MultiFunctionNode(
        // parameters
        INamedTypeSymbol enumerableType,
        IReadOnlyList<ITypeSymbol> parameters,
        IContainerNode parentContainer,
        IRangeNode parentRange,
        IReferenceGenerator referenceGenerator,
        
        // dependencies
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
        _typeSymbolUtility = typeSymbolUtility;
        _checkTypeProperties = checkTypeProperties;
        NamePrefix = $"CreateMulti{_enumerableType.Name}";
        NameNumberSuffix = referenceGenerator.Generate("");
    }

    private static IElementNode MapToReturnedElement(IElementNodeMapperBase mapper, ITypeSymbol itemType) =>
        mapper.Map(itemType, new(ImmutableStack<INamedTypeSymbol>.Empty, null));
    
    public override void Build(PassedContext passedContext)
    {
        base.Build(passedContext);
        var itemType = CollectionUtility.GetCollectionsInnerType(_enumerableType);
        var unwrappedItemType = _typeSymbolUtility.GetUnwrappedType(itemType);

        var concreteItemTypes = unwrappedItemType is INamedTypeSymbol namedTypeSymbol
            ? _checkTypeProperties.MapToImplementations(namedTypeSymbol, passedContext.InjectionKeyModification)
            : (IReadOnlyList<ITypeSymbol>) [unwrappedItemType];

        ReturnedElements = concreteItemTypes
            .Select(cit => MapToReturnedElement(
                GetMapper(unwrappedItemType, cit),
                itemType))
            .ToList();
    }

    protected override string NamePrefix { get; set; }
    protected override string NameNumberSuffix { get; set; }
}