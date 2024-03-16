using MrMeeseeks.DIE.Mappers.MappingParts;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Ranges;

namespace MrMeeseeks.DIE.Mappers;

internal interface IElementNodeMapper : IElementNodeMapperBase;

internal sealed class ElementNodeMapper : ElementNodeMapperBase, IElementNodeMapper
{
    internal ElementNodeMapper(
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
    }

    protected override IElementNodeMapperBase NextForWraps => this;
    protected override IElementNodeMapperBase Next => this;
}