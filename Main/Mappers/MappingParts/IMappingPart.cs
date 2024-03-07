using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Nodes.Elements;

namespace MrMeeseeks.DIE.Mappers.MappingParts;

internal interface IMappingPart
{
    IElementNode? Map(MappingPartData data);
}

internal record MappingPartData(
    ITypeSymbol Type,
    PassedContext PassedContext,
    IElementNodeMapperBase Next,
    IElementNodeMapperBase NextForWraps,
    IElementNodeMapperBase Current,
    Func<MapperData> GetMapperDataForAsyncWrapping);