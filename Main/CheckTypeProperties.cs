namespace MrMeeseeks.DIE;

public interface ICheckTypeProperties
{
    bool ShouldBeManaged(INamedTypeSymbol type);
    bool ShouldBeSingleInstance(INamedTypeSymbol type);
    bool ShouldBeScopedInstance(INamedTypeSymbol type);
    bool ShouldBeScopeRoot(INamedTypeSymbol type);
}

internal class CheckTypeProperties : ICheckTypeProperties
{
    private readonly IImmutableSet<ISymbol?> _transientTypes;
    private readonly IImmutableSet<ISymbol?> _singleInstanceTypes;
    private readonly IImmutableSet<ISymbol?> _scopedInstanceTypes;
    private readonly IImmutableSet<ISymbol?> _scopeRootTypes;

    public CheckTypeProperties(
        IGetAllImplementations getAllImplementations,
        ITypesFromAttributes typesFromAttributes)
    {
        _transientTypes = GetSetOfTypesWithProperties(typesFromAttributes.Transient);
        _singleInstanceTypes = GetSetOfTypesWithProperties(typesFromAttributes.SingleInstance);
        _scopedInstanceTypes = GetSetOfTypesWithProperties(typesFromAttributes.ScopedInstance);
        _scopeRootTypes = GetSetOfTypesWithProperties(typesFromAttributes.ScopeRoot);
            
        IImmutableSet<ISymbol?> GetSetOfTypesWithProperties(IReadOnlyList<INamedTypeSymbol> propertyGivingTypes) => getAllImplementations
            .AllImplementations
            .Where(i =>
            {
                var derivedTypes = AllDerivedTypes(i);
                return propertyGivingTypes.Any(t => derivedTypes.Contains(t, SymbolEqualityComparer.Default));
            })
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

    public bool ShouldBeManaged(INamedTypeSymbol type) => !_transientTypes.Contains(type);
    public bool ShouldBeSingleInstance(INamedTypeSymbol type) => _singleInstanceTypes.Contains(type);
    public bool ShouldBeScopedInstance(INamedTypeSymbol type) => _scopedInstanceTypes.Contains(type);
    public bool ShouldBeScopeRoot(INamedTypeSymbol type) => _scopeRootTypes.Contains(type);
}