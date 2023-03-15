namespace MrMeeseeks.DIE.Nodes.Mappers;

internal abstract record MapperData;

internal record VanillaMapperData : MapperData;
internal record OverridingMapperData(ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)> Override) : MapperData;
internal record OverridingWithDecorationMapper((INamedTypeSymbol, INamedTypeSymbol) Override) : MapperData;

internal interface IMapperFactory
{
    IElementNodeMapperBase Create(MapperData data);
}

internal class MapperFactory : IMapperFactory
{
    private readonly Func<IElementNodeMapper> _typeToElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)>, IOverridingElementNodeMapper> _overridingElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, (INamedTypeSymbol, INamedTypeSymbol), IOverridingElementNodeWithDecorationMapper> _overridingElementNodeWithDecorationMapperFactory;

    public MapperFactory(
        Func<IElementNodeMapper> typeToElementNodeMapperFactory, 
        Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)>, IOverridingElementNodeMapper> overridingElementNodeMapperFactory, 
        Func<IElementNodeMapperBase, (INamedTypeSymbol, INamedTypeSymbol), IOverridingElementNodeWithDecorationMapper> overridingElementNodeWithDecorationMapperFactory)
    {
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        _overridingElementNodeMapperFactory = overridingElementNodeMapperFactory;
        _overridingElementNodeWithDecorationMapperFactory = overridingElementNodeWithDecorationMapperFactory;
    }

    public IElementNodeMapperBase Create(MapperData data)
    {
        return data switch
        {
            VanillaMapperData => _typeToElementNodeMapperFactory(),
            OverridingMapperData overriding =>
                _overridingElementNodeMapperFactory(_typeToElementNodeMapperFactory(), overriding.Override),
            OverridingWithDecorationMapper overridingWithDecoration =>
                _overridingElementNodeWithDecorationMapperFactory(
                    _typeToElementNodeMapperFactory(),
                    overridingWithDecoration.Override),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}