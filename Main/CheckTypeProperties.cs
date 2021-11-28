namespace MrMeeseeks.DIE;

public interface ICheckTypeProperties
{
    bool ShouldBeManaged(INamedTypeSymbol implementationType);
    bool ShouldBeSingleInstance(INamedTypeSymbol implementationType);
    bool ShouldBeScopedInstance(INamedTypeSymbol implementationType);
    bool ShouldBeScopeRoot(INamedTypeSymbol implementationType);
    bool ShouldBeDecorated(INamedTypeSymbol interfaceType);
    bool IsDecorator(INamedTypeSymbol implementationType);
    IReadOnlyList<INamedTypeSymbol> GetDecorators(INamedTypeSymbol interfaceType);
}

internal class CheckTypeProperties : ICheckTypeProperties
{
    private readonly IImmutableSet<ISymbol?> _transientTypes;
    private readonly IImmutableSet<ISymbol?> _singleInstanceTypes;
    private readonly IImmutableSet<ISymbol?> _scopedInstanceTypes;
    private readonly IImmutableSet<ISymbol?> _scopeRootTypes;
    private readonly IImmutableSet<ISymbol?> _decoratorTypes;
    private readonly IDictionary<ISymbol?, List<INamedTypeSymbol>> _interfaceToDecorators;

    public CheckTypeProperties(
        IGetAllImplementations getAllImplementations,
        ITypesFromAttributes typesFromAttributes)
    {
        _transientTypes = GetSetOfTypesWithProperties(typesFromAttributes.Transient);
        _singleInstanceTypes = GetSetOfTypesWithProperties(typesFromAttributes.SingleInstance);
        _scopedInstanceTypes = GetSetOfTypesWithProperties(typesFromAttributes.ScopedInstance);
        _scopeRootTypes = GetSetOfTypesWithProperties(typesFromAttributes.ScopeRoot);
        _decoratorTypes = GetSetOfTypesWithProperties(typesFromAttributes.Decorator);
        _interfaceToDecorators = _decoratorTypes
            .OfType<INamedTypeSymbol>()
            .GroupBy(nts =>
            {
                var namedTypeSymbol = nts.AllInterfaces
                    .Single(t =>
                        typesFromAttributes.Decorator.Contains(t.OriginalDefinition, SymbolEqualityComparer.Default));
                return namedTypeSymbol.TypeArguments.First();
            }, SymbolEqualityComparer.Default)
            .ToDictionary(g => g.Key, g => g.ToList(), SymbolEqualityComparer.Default);

        IImmutableSet<ISymbol?> GetSetOfTypesWithProperties(IReadOnlyList<INamedTypeSymbol> propertyGivingTypes) => getAllImplementations
            .AllImplementations
            .Where(i =>
            {
                var derivedTypes = AllDerivedTypes(i).Select(t => t.OriginalDefinition).ToList();
                return propertyGivingTypes.Any(t => derivedTypes.Contains(t.OriginalDefinition, SymbolEqualityComparer.Default));
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

    public bool ShouldBeManaged(INamedTypeSymbol implementationType) => !_transientTypes.Contains(implementationType);
    public bool ShouldBeSingleInstance(INamedTypeSymbol implementationType) => _singleInstanceTypes.Contains(implementationType);
    public bool ShouldBeScopedInstance(INamedTypeSymbol implementationType) => _scopedInstanceTypes.Contains(implementationType);
    public bool ShouldBeScopeRoot(INamedTypeSymbol implementationType) => _scopeRootTypes.Contains(implementationType);
    public bool ShouldBeDecorated(INamedTypeSymbol interfaceType) => _interfaceToDecorators.ContainsKey(interfaceType);
    public bool IsDecorator(INamedTypeSymbol implementationType) => _decoratorTypes.Contains(implementationType);
    public IReadOnlyList<INamedTypeSymbol> GetDecorators(INamedTypeSymbol interfaceType) => 
        _interfaceToDecorators.TryGetValue(interfaceType, out var ret)
            ? ret 
            : Array.Empty<INamedTypeSymbol>();
}