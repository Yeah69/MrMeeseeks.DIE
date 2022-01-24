namespace MrMeeseeks.DIE.Configuration;

internal interface ITypesFromAttributes
{
    IReadOnlyList<INamedTypeSymbol> Spy { get; }
    IReadOnlyList<INamedTypeSymbol> Implementation { get; }
    IReadOnlyList<INamedTypeSymbol> Transient { get; }
    IReadOnlyList<INamedTypeSymbol> ContainerInstance { get; }
    IReadOnlyList<INamedTypeSymbol> TransientScopeInstance { get; }
    IReadOnlyList<INamedTypeSymbol> ScopeInstance { get; }
    IReadOnlyList<INamedTypeSymbol> TransientScopeRoot { get; }
    IReadOnlyList<INamedTypeSymbol> ScopeRoot { get; }
    IReadOnlyList<INamedTypeSymbol> Decorator { get; }
    IReadOnlyList<INamedTypeSymbol> Composite { get; }
    IReadOnlyList<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> DecoratorSequenceChoices { get; }
    IReadOnlyList<(INamedTypeSymbol, IMethodSymbol)> ConstructorChoices { get; }
}

internal class TypesFromAttributes : ScopeTypesFromAttributes
{
    internal TypesFromAttributes(
        // parameter
        IReadOnlyList<AttributeData> attributeData,

        // dependencies
        WellKnownTypes wellKnownTypes) : base(attributeData, wellKnownTypes)
    {
        ContainerInstance = GetTypesFromAttribute(wellKnownTypes.ContainerInstanceAggregationAttribute).ToList();
        TransientScopeInstance = GetTypesFromAttribute(wellKnownTypes.TransientScopeInstanceAggregationAttribute).ToList();
        TransientScopeRoot = GetTypesFromAttribute(wellKnownTypes.TransientScopeRootAggregationAttribute).ToList();
        ScopeRoot = GetTypesFromAttribute(wellKnownTypes.ScopeRootAggregationAttribute).ToList();
    }
}

internal class ScopeTypesFromAttributes : ITypesFromAttributes
{
    internal ScopeTypesFromAttributes(
        // parameter
        IReadOnlyList<AttributeData> attributeData,

        // dependencies
        WellKnownTypes wellKnownTypes)
    {
        AttributeDictionary = attributeData
            .GroupBy(ad => ad.AttributeClass, SymbolEqualityComparer.Default)
            .ToDictionary(g => g.Key, g => g);

        Spy = GetTypesFromAttribute(wellKnownTypes.SpyAggregationAttribute).ToList();
        Implementation = GetTypesFromAttribute(wellKnownTypes.ImplementationAggregationAttribute).ToList();
        Transient = GetTypesFromAttribute(wellKnownTypes.TransientAggregationAttribute).ToList();
        ContainerInstance = new List<INamedTypeSymbol>();
        TransientScopeInstance = new List<INamedTypeSymbol>();
        ScopeInstance = GetTypesFromAttribute(wellKnownTypes.ScopeInstanceAggregationAttribute).ToList();
        TransientScopeRoot = new List<INamedTypeSymbol>();
        ScopeRoot = new List<INamedTypeSymbol>();
        Decorator = GetTypesFromAttribute(wellKnownTypes.DecoratorAggregationAttribute).ToList();
        Composite = GetTypesFromAttribute(wellKnownTypes.CompositeAggregationAttribute).ToList();
        
        DecoratorSequenceChoices = (AttributeDictionary.TryGetValue(wellKnownTypes.DecoratorSequenceChoiceAttribute, out var group) ? group : Enumerable.Empty<AttributeData>())
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

        ConstructorChoices = (AttributeDictionary.TryGetValue(wellKnownTypes.ConstructorChoiceAttribute, out var group0) ? group0 : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 2)
                    return null;
                var implementationType = ad.ConstructorArguments[0].Value as INamedTypeSymbol;
                var parameterTypes = ad
                    .ConstructorArguments[1]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<INamedTypeSymbol>()
                    .ToList();

                if (implementationType is { })
                {
                    var constructorChoice = implementationType
                        .Constructors
                        .Where(c => c.Parameters.Length == parameterTypes.Count)
                        .SingleOrDefault(c => c.Parameters.Select(p => p.Type)
                            .Zip(parameterTypes,
                                (pLeft, pRight) => pLeft.Equals(pRight, SymbolEqualityComparer.Default))
                            .All(b => b));
                    return constructorChoice is { }
                        ? (implementationType, constructorChoice)
                        : ((INamedTypeSymbol, IMethodSymbol)?)null;
                }

                return null;
            })
            .OfType<(INamedTypeSymbol, IMethodSymbol)>()
            .ToList();
    }

    private IReadOnlyDictionary<ISymbol?, IGrouping<ISymbol?, AttributeData>> AttributeDictionary { get; }
    
    protected IEnumerable<INamedTypeSymbol> GetTypesFromAttribute(
        INamedTypeSymbol attribute)
    {
        return (AttributeDictionary.TryGetValue(attribute, out var group1) ? group1 : Enumerable.Empty<AttributeData>())
            .SelectMany(ad => ad.ConstructorArguments
                .Where(tc => tc.Kind == TypedConstantKind.Type)
                .OfType<TypedConstant>()
                .Concat(ad.ConstructorArguments.SelectMany(ca => ca.Kind is TypedConstantKind.Array
                    ? (IEnumerable<TypedConstant>)ca.Values
                    : Array.Empty<TypedConstant>())))
            .Select(tc => !CheckValidType(tc, out var type) ? null : type)
            .Where(t => t is not null)
            .OfType<INamedTypeSymbol>()
            .Select(t => t.OriginalDefinition);
        
        bool CheckValidType(TypedConstant typedConstant, out INamedTypeSymbol type)
        {
            type = (typedConstant.Value as INamedTypeSymbol)!;
            return typedConstant.Value is not null;
        }
    }

    public IReadOnlyList<INamedTypeSymbol> Spy { get; }
    public IReadOnlyList<INamedTypeSymbol> Implementation { get; }
    public IReadOnlyList<INamedTypeSymbol> Transient { get; }
    public IReadOnlyList<INamedTypeSymbol> ContainerInstance { get; protected init; }
    public IReadOnlyList<INamedTypeSymbol> TransientScopeInstance { get; protected init; }
    public IReadOnlyList<INamedTypeSymbol> ScopeInstance { get; }
    public IReadOnlyList<INamedTypeSymbol> TransientScopeRoot { get; protected init; }
    public IReadOnlyList<INamedTypeSymbol> ScopeRoot { get; protected init; }
    public IReadOnlyList<INamedTypeSymbol> Decorator { get; }
    public IReadOnlyList<INamedTypeSymbol> Composite { get; }
    public IReadOnlyList<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> DecoratorSequenceChoices { get; }
    public IReadOnlyList<(INamedTypeSymbol, IMethodSymbol)> ConstructorChoices { get; }
}