using MrMeeseeks.DIE.Mappers.MappingParts;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Mappers;

internal interface ITransientScopeDisposalElementNodeMapper : IElementNodeMapperBase;

internal sealed class TransientScopeDisposalElementNodeMapper : ElementNodeMapperBase, ITransientScopeDisposalElementNodeMapper
{
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Func<INamedTypeSymbol, ITransientScopeDisposalTriggerNode> _transientScopeDisposalTriggerNodeFactory;

    internal TransientScopeDisposalElementNodeMapper(
        IElementNodeMapperBase parentElementNodeMapper,
        
        IContainerNode parentContainer,
        WellKnownTypes wellKnownTypes,
        IOverridesMappingPart overridesMappingPart,
        IUserDefinedElementsMappingPart userDefinedElementsMappingPart,
        IAsyncWrapperMappingPart asyncWrapperMappingPart,
        ITupleMappingPart tupleMappingPart,
        IDelegateMappingPart delegateMappingPart,
        ICollectionMappingPart collectionMappingPart,
        IAbstractionImplementationMappingPart abstractionImplementationMappingPart,
        Func<ITypeSymbol, IOutParameterNode> outParameterNodeFactory,
        Func<string, ITypeSymbol, IErrorNode> errorNodeFactory,
        Func<INamedTypeSymbol, ITransientScopeDisposalTriggerNode> transientScopeDisposalTriggerNodeFactory) 
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
        _wellKnownTypes = wellKnownTypes;
        _transientScopeDisposalTriggerNodeFactory = transientScopeDisposalTriggerNodeFactory;
        Next = parentElementNodeMapper;
    }

    protected override IElementNodeMapperBase NextForWraps => this;

    protected override IElementNodeMapperBase Next { get; }

    public override IElementNode Map(ITypeSymbol type, PassedContext passedContext)
    {
        if (type is INamedTypeSymbol namedType
            && (CustomSymbolEqualityComparer.Default.Equals(namedType, _wellKnownTypes.IDisposable)
                || CustomSymbolEqualityComparer.Default.Equals(namedType, _wellKnownTypes.IAsyncDisposable)))
            return _transientScopeDisposalTriggerNodeFactory(namedType);

        return base.Map(type, passedContext);
    }
}