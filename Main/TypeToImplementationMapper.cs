namespace MrMeeseeks.DIE;

internal interface ITypeToImplementationsMapper
{
    IList<INamedTypeSymbol> Map(ITypeSymbol typeSymbol); 
}

internal class TypeToImplementationsMapper : ITypeToImplementationsMapper
{
    private readonly Dictionary<ITypeSymbol, List<INamedTypeSymbol>> _map;

    public TypeToImplementationsMapper(
        IGetAllImplementations getAllImplementations) =>
        _map = getAllImplementations
            .AllImplementations
            .SelectMany(i => { return i.AllInterfaces.OfType<ITypeSymbol>().Select(ii => (ii, i)).Prepend((i, i)); })
            .GroupBy(t => t.Item1, t => t.Item2)
            .ToDictionary(g => g.Key, g => g.Distinct().ToList());

    public IList<INamedTypeSymbol> Map(ITypeSymbol typeSymbol) =>
        _map.TryGetValue(typeSymbol, out var implementations) 
            ? implementations 
            : new List<INamedTypeSymbol>();
}