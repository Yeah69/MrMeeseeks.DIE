namespace MrMeeseeks.DIE.Mappers;

internal interface IMapperFactory
{
    IElementNodeMapperBase Create(MapperData data);
}

internal sealed class MapperFactory : IMapperFactory
{
    private readonly Func<IElementNodeMapper> _typeToElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, Override)>, IOverridingElementNodeMapper> _overridingElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, (INamedTypeSymbol, INamedTypeSymbol), IOverridingElementNodeWithDecorationMapper> _overridingElementNodeWithDecorationMapperFactory;

    public MapperFactory(
        Func<IElementNodeMapper> typeToElementNodeMapperFactory, 
        Func<IElementNodeMapperBase, ImmutableQueue<(INamedTypeSymbol, Override)>, IOverridingElementNodeMapper> overridingElementNodeMapperFactory, 
        Func<IElementNodeMapperBase, (INamedTypeSymbol, INamedTypeSymbol), IOverridingElementNodeWithDecorationMapper> overridingElementNodeWithDecorationMapperFactory)
    {
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        _overridingElementNodeMapperFactory = overridingElementNodeMapperFactory;
        _overridingElementNodeWithDecorationMapperFactory = overridingElementNodeWithDecorationMapperFactory;
    }

    public IElementNodeMapperBase Create(MapperData data) =>
        data switch
        {
            VanillaMapperData => _typeToElementNodeMapperFactory(),
            OverridingMapperData overriding =>
                _overridingElementNodeMapperFactory(_typeToElementNodeMapperFactory(), overriding.Override),
            OverridingWithDecorationMapperData overridingWithDecoration =>
                _overridingElementNodeWithDecorationMapperFactory(
                    _typeToElementNodeMapperFactory(),
                    overridingWithDecoration.Override),
            _ => throw new ArgumentOutOfRangeException(nameof(data), $"Switch in DIE type {nameof(MapperFactory)} is not exhaustive.")
        };
}