namespace MrMeeseeks.DIE;

internal interface ICheckDecorators
{
    bool ShouldBeDecorated(INamedTypeSymbol interfaceType);
    bool IsDecorator(INamedTypeSymbol implementationType);
    IReadOnlyList<INamedTypeSymbol> GetSequenceFor(INamedTypeSymbol interfaceType, INamedTypeSymbol implementationType);
}

internal class CheckDecorators : ICheckDecorators
{
    private readonly IImmutableSet<ISymbol?> _decoratorTypes;
    private readonly IDictionary<ISymbol?, List<INamedTypeSymbol>> _interfaceToDecorators;
    private readonly IDictionary<INamedTypeSymbol,IReadOnlyList<INamedTypeSymbol>> _interfaceSequenceChoices;
    private readonly IDictionary<INamedTypeSymbol,IReadOnlyList<INamedTypeSymbol>> _implementationSequenceChoices;

    internal CheckDecorators(
        WellKnownTypes wellKnownTypes,
        IGetAssemblyAttributes getAssemblyAttributes,
        ITypesFromTypeAggregatingAttributes typesFromTypeAggregatingAttributes,
        IGetSetOfTypesWithProperties getSetOfTypesWithProperties)
    {
        _decoratorTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.Decorator);
        _interfaceToDecorators = _decoratorTypes
            .OfType<INamedTypeSymbol>()
            .GroupBy(nts =>
            {
                var namedTypeSymbol = nts.AllInterfaces
                    .Single(t =>
                        typesFromTypeAggregatingAttributes.Decorator.Contains(t.OriginalDefinition, SymbolEqualityComparer.Default));
                return namedTypeSymbol.TypeArguments.First();
            }, SymbolEqualityComparer.Default)
            .ToDictionary(g => g.Key, g => g.ToList(), SymbolEqualityComparer.Default);
        var sequenceChoices = getAssemblyAttributes
            .AllAssemblyAttributes
            .Where(ad =>
                ad.AttributeClass?.Equals(wellKnownTypes.DecoratorSequenceChoiceAttribute,
                    SymbolEqualityComparer.Default) ?? false)
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 2)
                    return null;
                var decoratedType = ad.ConstructorArguments[0].Value;
                var decorators = ad
                    .ConstructorArguments[1]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<INamedTypeSymbol>()
                    .ToList();
                
                return decoratedType is null 
                    ? null 
                    : ((INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)?) (decoratedType, decorators);
            })
            .OfType<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)>()
            .ToList();

        _interfaceSequenceChoices = sequenceChoices
            .Where(t => t.Item1.TypeKind == TypeKind.Interface)
            .ToDictionary(t => t.Item1, t => t.Item2);

        _implementationSequenceChoices = sequenceChoices
            .Where(t => t.Item1.TypeKind == TypeKind.Class || t.Item1.TypeKind == TypeKind.Struct)
            .ToDictionary(t => t.Item1, t => t.Item2);
    }
    
    public bool ShouldBeDecorated(INamedTypeSymbol interfaceType) => _interfaceToDecorators.ContainsKey(interfaceType);
    public bool IsDecorator(INamedTypeSymbol implementationType) => _decoratorTypes.Contains(implementationType);

    public IReadOnlyList<INamedTypeSymbol> GetSequenceFor(INamedTypeSymbol interfaceType, INamedTypeSymbol implementationType)
    {
        if (_implementationSequenceChoices.TryGetValue(implementationType, out var implementationSequence))
            return implementationSequence;
        if (_interfaceSequenceChoices.TryGetValue(interfaceType, out var interfaceSequence))
            return interfaceSequence;
        if (_interfaceToDecorators.TryGetValue(interfaceType, out var allDecorators)
            && allDecorators.Count == 1)
            return allDecorators;
        throw new Exception("Couldn't find unambiguous sequence of decorators");
    }
}