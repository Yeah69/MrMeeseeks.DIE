using MrMeeseeks.DIE.Mappers.MappingParts;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Mappers;

internal interface IOverridingElementNodeMapper : IElementNodeMapperBase;

internal sealed class OverridingElementNodeMapper : ElementNodeMapperBase, IOverridingElementNodeMapper
{
    private readonly ImmutableQueue<(INamedTypeSymbol InterfaceType, INamedTypeSymbol ImplementationType)> _override;
    private readonly Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)>, IOverridingElementNodeMapper> _overridingElementNodeMapperFactory;

    internal OverridingElementNodeMapper(
        IElementNodeMapperBase parentElementNodeMapper,
        ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)> @override,
        
        IContainerNode parentContainer,
        IOverridesMappingPart overridesMappingPart,
        IUserDefinedElementsMappingPart userDefinedElementsMappingPart,
        IAsyncWrapperMappingPart asyncWrapperMappingPart,
        ITupleMappingPart tupleMappingPart,
        IDelegateMappingPart delegateMappingPart,
        ICollectionMappingPart collectionMappingPart,
        IAbstractionImplementationMappingPart abstractionImplementationMappingPart,
        Func<ITypeSymbol, IOutParameterNode> outParameterNodeFactory,
        Func<string, (string Name, IElementNode Element)[], IImplicitScopeImplementationNode> implicitScopeImplementationNodeFactory,
        Func<string, IReferenceNode> referenceNodeFactory,
        Func<string, ITypeSymbol, IErrorNode> errorNodeFactory,
        Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)>, IOverridingElementNodeMapper> overridingElementNodeMapperFactory) 
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
            implicitScopeImplementationNodeFactory,
            referenceNodeFactory,
            errorNodeFactory)
    {
        Next = parentElementNodeMapper;
        _override = @override;
        _overridingElementNodeMapperFactory = overridingElementNodeMapperFactory;
    }

    protected override IElementNodeMapperBase NextForWraps => this;

    protected override IElementNodeMapperBase Next { get; }

    public override IElementNode Map(ITypeSymbol type, PassedContext passedContext)
    {
        if (!_override.IsEmpty
            && type is INamedTypeSymbol abstraction 
            && CustomSymbolEqualityComparer.Default.Equals(_override.Peek().InterfaceType, type))
        {
            var nextOverride = _override.Dequeue(out var currentOverride);
            var mapper = _overridingElementNodeMapperFactory(this, nextOverride);
            return SwitchImplementation(
                new(true, true, true),
                abstraction,
                currentOverride.ImplementationType,
                passedContext,
                mapper);
        }
        return base.Map(type, passedContext);
    }

    protected override MapperData GetMapperDataForAsyncWrapping() => new OverridingMapperData(_override);
}