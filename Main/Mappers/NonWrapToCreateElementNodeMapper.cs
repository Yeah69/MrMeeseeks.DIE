using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Mappers.MappingParts;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Mappers;

internal interface INonWrapToCreateElementNodeMapper : IElementNodeMapperBase;

internal sealed class NonWrapToCreateElementNodeMapper : ElementNodeMapperBase, INonWrapToCreateElementNodeMapper
{
    private readonly IFunctionNode _parentFunction;
    private readonly IRangeNode _parentRange;
    private readonly WellKnownTypes _wellKnownTypes;

    internal NonWrapToCreateElementNodeMapper(
        IElementNodeMapperBase parentElementNodeMapper,
        
        IFunctionNode parentFunction,
        IContainerNode parentContainer,
        ITransientScopeWideContext transientScopeWideContext,
        IContainerWideContext containerWideContext,
        IOverridesMappingPart overridesMappingPart,
        IUserDefinedElementsMappingPart userDefinedElementsMappingPart,
        IAsyncWrapperMappingPart asyncWrapperMappingPart,
        ITupleMappingPart tupleMappingPart,
        IDelegateMappingPart delegateMappingPart,
        ICollectionMappingPart collectionMappingPart,
        IAbstractionImplementationMappingPart abstractionImplementationMappingPart,
        Func<ITypeSymbol, IOutParameterNode> outParameterNodeFactory,
        Func<string, ITypeSymbol, IErrorNode> errorNodeFactory) 
        : base(
            parentContainer,
            overridesMappingPart,
            userDefinedElementsMappingPart,
            asyncWrapperMappingPart,
            tupleMappingPart,
            delegateMappingPart,
            collectionMappingPart,
            abstractionImplementationMappingPart,
            outParameterNodeFactory,
            errorNodeFactory)
    {
        _parentFunction = parentFunction;
        _parentRange = transientScopeWideContext.Range;
        Next = parentElementNodeMapper;
        _wellKnownTypes = containerWideContext.WellKnownTypes;
    }

    protected override IElementNodeMapperBase NextForWraps => this;

    protected override IElementNodeMapperBase Next { get; }

    public override IElementNode Map(ITypeSymbol type, PassedContext passedContext)
    {
        if (type is INamedTypeSymbol namedType && _parentRange.GetInitializedNode(namedType) is { } initializedNode)
            return initializedNode;
        
        return TypeSymbolUtility.IsWrapType(type, _wellKnownTypes)
            ? base.Map(type, passedContext)
            : _parentRange.BuildCreateCall(type, _parentFunction);
    }
}