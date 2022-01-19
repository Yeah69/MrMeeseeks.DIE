namespace MrMeeseeks.DIE;

internal interface ITypesFromTypeAggregatingAttributes
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
}

internal class TypesFromTypeAggregatingAttributes : ITypesFromTypeAggregatingAttributes
{
    internal TypesFromTypeAggregatingAttributes(
        WellKnownTypes wellKnownTypes,
        IGetAssemblyAttributes getAssemblyAttributes)
    {
        Spy = GetTypesFromAttribute(wellKnownTypes.SpyAggregationAttribute).ToList();
        Implementation = GetTypesFromAttribute(wellKnownTypes.ImplementationAggregationAttribute).ToList();
        Transient = GetTypesFromAttribute(wellKnownTypes.TransientAggregationAttribute).ToList();
        ContainerInstance = GetTypesFromAttribute(wellKnownTypes.ContainerInstanceAggregationAttribute).ToList();
        TransientScopeInstance = GetTypesFromAttribute(wellKnownTypes.TransientScopeInstanceAggregationAttribute).ToList();
        ScopeInstance = GetTypesFromAttribute(wellKnownTypes.ScopeInstanceAggregationAttribute).ToList();
        TransientScopeRoot = GetTypesFromAttribute(wellKnownTypes.TransientScopeRootAggregationAttribute).ToList();
        ScopeRoot = GetTypesFromAttribute(wellKnownTypes.ScopeRootAggregationAttribute).ToList();
        Decorator = GetTypesFromAttribute(wellKnownTypes.DecoratorAggregationAttribute).ToList();
        Composite = GetTypesFromAttribute(wellKnownTypes.CompositeAggregationAttribute).ToList();
            
        IEnumerable<INamedTypeSymbol> GetTypesFromAttribute(INamedTypeSymbol attribute) => getAssemblyAttributes
            .AllAssemblyAttributes
            .Where(ad =>
                ad.AttributeClass?.Equals(attribute, SymbolEqualityComparer.Default) ?? false)
            .SelectMany(ad => ad.ConstructorArguments
                .Where(tc => tc.Kind == TypedConstantKind.Type)
                .OfType<TypedConstant>()
                .Concat(ad.ConstructorArguments.SelectMany(ca => ca.Kind is TypedConstantKind.Array 
                    ? (IEnumerable<TypedConstant>)ca.Values 
                    : Array.Empty<TypedConstant>())))
            .Select(tc =>
            {
                if (!CheckValidType(tc, out var type))
                {
                    return null;
                }

                return type;
            })
            .Where(t => t is not null)
            .OfType<INamedTypeSymbol>()
            .Select(t => t.OriginalDefinition);

        bool CheckValidType(TypedConstant typedConstant, out INamedTypeSymbol type)
        {
            type = (typedConstant.Value as INamedTypeSymbol)!;
            if (typedConstant.Value is null)
                return false;

            return true;
        }
    }
        
    public IReadOnlyList<INamedTypeSymbol> Spy { get; }
    public IReadOnlyList<INamedTypeSymbol> Implementation { get; }
    public IReadOnlyList<INamedTypeSymbol> Transient { get; }
    public IReadOnlyList<INamedTypeSymbol> ContainerInstance { get; }
    public IReadOnlyList<INamedTypeSymbol> TransientScopeInstance { get; }
    public IReadOnlyList<INamedTypeSymbol> ScopeInstance { get; }
    public IReadOnlyList<INamedTypeSymbol> TransientScopeRoot { get; }
    public IReadOnlyList<INamedTypeSymbol> ScopeRoot { get; }
    public IReadOnlyList<INamedTypeSymbol> Decorator { get; }
    public IReadOnlyList<INamedTypeSymbol> Composite { get; }
}