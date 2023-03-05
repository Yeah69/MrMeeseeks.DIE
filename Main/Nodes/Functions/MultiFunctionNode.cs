using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IMultiFunctionNode : IFunctionNode
{
    IReadOnlyList<IElementNode> ReturnedElements { get; }
    bool IsAsyncEnumerable { get; }
    string ItemTypeFullName { get; }
}

internal class MultiFunctionNode : ReturningFunctionNodeBase, IMultiFunctionNode, IScopeInstance
{
    private readonly INamedTypeSymbol _enumerableType;
    private readonly IRangeNode _parentNode;
    private readonly IContainerNode _parentContainer;
    private readonly IUserDefinedElementsBase _userDefinedElementsBase;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly Func<IFunctionNode, IRangeNode, IContainerNode, IUserDefinedElementsBase, ICheckTypeProperties, IReferenceGenerator, IElementNodeMapper> _typeToElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, ElementNodeMapperBase.PassedDependencies, (INamedTypeSymbol, INamedTypeSymbol), IOverridingElementNodeWithDecorationMapper> _overridingElementNodeWithDecorationMapperFactory;
    private readonly WellKnownTypes _wellKnownTypes;

    internal MultiFunctionNode(
        // parameters
        INamedTypeSymbol enumerableType,
        IReadOnlyList<ITypeSymbol> parameters,
        IRangeNode parentNode,
        IContainerNode parentContainer,
        IUserDefinedElementsBase userDefinedElements,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        
        // dependencies
        Func<ITypeSymbol, IReferenceGenerator, IParameterNode> parameterNodeFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<string, string, IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IContainerNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<IFunctionNode, IRangeNode, IContainerNode, IUserDefinedElementsBase, ICheckTypeProperties, IReferenceGenerator, IElementNodeMapper> typeToElementNodeMapperFactory,
        Func<IElementNodeMapperBase, ElementNodeMapperBase.PassedDependencies, (INamedTypeSymbol, INamedTypeSymbol), IOverridingElementNodeWithDecorationMapper> overridingElementNodeWithDecorationMapperFactory,
        IContainerWideContext containerWideContext)
        : base(
            Microsoft.CodeAnalysis.Accessibility.Private, 
            enumerableType, 
            parameters, 
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
            parentContainer, 
            parentNode,
            referenceGenerator,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            containerWideContext)
    {
        _enumerableType = enumerableType;
        _parentNode = parentNode;
        _parentContainer = parentContainer;
        _userDefinedElementsBase = userDefinedElements;
        _checkTypeProperties = checkTypeProperties;
        _referenceGenerator = referenceGenerator;
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        _overridingElementNodeWithDecorationMapperFactory = overridingElementNodeWithDecorationMapperFactory;
        _wellKnownTypes = containerWideContext.WellKnownTypes;

        ItemTypeFullName = CollectionUtility.GetCollectionsInnerType(enumerableType).FullName();

        SuppressAsync =
            CustomSymbolEqualityComparer.Default.Equals(enumerableType.OriginalDefinition, containerWideContext.WellKnownTypesCollections.IAsyncEnumerable1);
        IsAsyncEnumerable =
            CustomSymbolEqualityComparer.Default.Equals(enumerableType.OriginalDefinition, containerWideContext.WellKnownTypesCollections.IAsyncEnumerable1);

        Name = referenceGenerator.Generate("CreateMulti", enumerableType);
    }

    private IElementNodeMapperBase GetMapper(
        ITypeSymbol unwrappedType,
        ITypeSymbol concreteImplementationType,
        IMultiFunctionNode parentFunction,
        IRangeNode parentNode,
        IContainerNode parentContainer,
        IUserDefinedElementsBase userDefinedElements,
        ICheckTypeProperties checkTypeProperties)
    {
        var baseMapper = _typeToElementNodeMapperFactory(parentFunction, parentNode, parentContainer, userDefinedElements, checkTypeProperties, _referenceGenerator);
        return concreteImplementationType is INamedTypeSymbol namedTypeSymbol && unwrappedType is INamedTypeSymbol namedUnwrappedType
            ? _overridingElementNodeWithDecorationMapperFactory(
                baseMapper,
                baseMapper.MapperDependencies,
                (namedUnwrappedType, namedTypeSymbol))
            : baseMapper;
    }

    protected override bool SuppressAsync { get; }

    private IElementNode MapToReturnedElement(IElementNodeMapperBase mapper, ITypeSymbol itemType) =>
        mapper.Map(itemType, ImmutableStack.Create<INamedTypeSymbol>());
    
    public override void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        var itemType = CollectionUtility.GetCollectionsInnerType(_enumerableType);
        var unwrappedItemType = TypeSymbolUtility.GetUnwrappedType(itemType, _wellKnownTypes);

        var concreteItemTypes = unwrappedItemType is INamedTypeSymbol namedTypeSymbol
            ? _checkTypeProperties.MapToImplementations(namedTypeSymbol)
            : (IReadOnlyList<ITypeSymbol>) new[] { unwrappedItemType };

        ReturnedElements = concreteItemTypes
            .Select(cit => MapToReturnedElement(
                GetMapper(unwrappedItemType, cit, this, _parentNode, _parentContainer, _userDefinedElementsBase, _checkTypeProperties),
                itemType))
            .ToList();
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitMultiFunctionNode(this);

    public override string Name { get; protected set; }

    public IReadOnlyList<IElementNode> ReturnedElements { get; private set; } = Array.Empty<IElementNode>();
    public bool IsAsyncEnumerable { get; }
    public string ItemTypeFullName { get; }
}