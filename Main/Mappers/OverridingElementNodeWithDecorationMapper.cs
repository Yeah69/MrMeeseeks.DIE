using MrMeeseeks.DIE.Mappers.MappingParts;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Mappers;

internal interface IOverridingElementNodeWithDecorationMapper : IElementNodeMapperBase;

internal sealed class OverridingElementNodeWithDecorationMapper : ElementNodeMapperBase, IOverridingElementNodeWithDecorationMapper
{
    private readonly (INamedTypeSymbol InterfaceType, INamedTypeSymbol ImplementationType) _override;

    internal OverridingElementNodeWithDecorationMapper(
        IElementNodeMapperBase parentElementNodeMapper,
        (INamedTypeSymbol, INamedTypeSymbol) @override,
        
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
            implicitScopeImplementationNodeFactory,
            referenceNodeFactory,
            errorNodeFactory)
    {
        Next = parentElementNodeMapper;
        _override = @override;
    }

    protected override IElementNodeMapperBase NextForWraps => this;

    protected override IElementNodeMapperBase Next { get; }

    public override IElementNode Map(ITypeSymbol type, PassedContext passedContext) =>
        CustomSymbolEqualityComparer.Default.Equals(_override.InterfaceType, type) 
        && type is INamedTypeSymbol abstractionType
            ? SwitchInterfaceWithPotentialDecoration(
                abstractionType, 
                _override.ImplementationType, 
                passedContext,
                Next)
            : base.Map(type, passedContext);

    protected override MapperData GetMapperDataForAsyncWrapping() => new OverridingWithDecorationMapperData(_override);
}