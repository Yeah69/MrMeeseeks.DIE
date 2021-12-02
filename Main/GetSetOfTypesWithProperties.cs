namespace MrMeeseeks.DIE;

public interface IGetSetOfTypesWithProperties
{
    IImmutableSet<ISymbol?> Get(IReadOnlyList<INamedTypeSymbol> propertyGivingTypes);
}

internal class GetSetOfTypesWithProperties : IGetSetOfTypesWithProperties
{
    private readonly IGetAllImplementations _getAllImplementations;

    public GetSetOfTypesWithProperties(IGetAllImplementations getAllImplementations)
    {
        _getAllImplementations = getAllImplementations;
    }
    
    public IImmutableSet<ISymbol?> Get(IReadOnlyList<INamedTypeSymbol> propertyGivingTypes)
    {
        return _getAllImplementations
            .AllImplementations
            .Where(i =>
            {
                var derivedTypes = AllDerivedTypes(i).Select(t => t.OriginalDefinition).ToList();
                return propertyGivingTypes.Any(t =>
                    derivedTypes.Contains(t.OriginalDefinition, SymbolEqualityComparer.Default));
            })
            .Distinct(SymbolEqualityComparer.Default)
            .ToImmutableHashSet(SymbolEqualityComparer.Default);
        

        IEnumerable<INamedTypeSymbol> AllDerivedTypes(INamedTypeSymbol type)
        {
            var concreteTypes = new List<INamedTypeSymbol>();
            var temp = type;
            while (temp is {})
            {
                concreteTypes.Add(temp);
                temp = temp.BaseType;
            }
            return type
                .AllInterfaces
                .Append(type)
                .Concat(concreteTypes);
        }
    }
}