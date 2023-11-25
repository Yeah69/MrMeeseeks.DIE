namespace MrMeeseeks.DIE.Nodes.Mappers;

internal interface IMapperFactory
{
    IElementNodeMapperBase Create(MapperData data);
}

internal class MapperFactory(Func<IElementNodeMapper> typeToElementNodeMapperFactory,
        Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)>, IOverridingElementNodeMapper>
            overridingElementNodeMapperFactory,
        Func<IElementNodeMapperBase, (INamedTypeSymbol, INamedTypeSymbol), IOverridingElementNodeWithDecorationMapper>
            overridingElementNodeWithDecorationMapperFactory)
    : IMapperFactory
{
    public IElementNodeMapperBase Create(MapperData data)
    {
        return data switch
        {
            VanillaMapperData => typeToElementNodeMapperFactory(),
            OverridingMapperData overriding =>
                overridingElementNodeMapperFactory(typeToElementNodeMapperFactory(), overriding.Override),
            OverridingWithDecorationMapperData overridingWithDecoration =>
                overridingElementNodeWithDecorationMapperFactory(
                    typeToElementNodeMapperFactory(),
                    overridingWithDecoration.Override),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}