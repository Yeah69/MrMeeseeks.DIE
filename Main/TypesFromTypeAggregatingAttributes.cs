namespace MrMeeseeks.DIE;

public interface ITypesFromTypeAggregatingAttributes
{
    IReadOnlyList<INamedTypeSymbol> Spy { get; }
    IReadOnlyList<INamedTypeSymbol> Transient { get; }
    IReadOnlyList<INamedTypeSymbol> SingleInstance { get; }
    IReadOnlyList<INamedTypeSymbol> ScopedInstance { get; }
    IReadOnlyList<INamedTypeSymbol> ScopeRoot { get; }
    IReadOnlyList<INamedTypeSymbol> Decorator { get; }
}

internal class TypesFromTypeAggregatingAttributes : ITypesFromTypeAggregatingAttributes
{
    public TypesFromTypeAggregatingAttributes(
        WellKnownTypes wellKnownTypes,
        IGetAssemblyAttributes getAssemblyAttributes)
    {
        Spy = GetTypesFromAttribute(wellKnownTypes.SpyAggregationAttribute).ToList();
        Transient = GetTypesFromAttribute(wellKnownTypes.TransientAggregationAttribute).ToList();
        SingleInstance = GetTypesFromAttribute(wellKnownTypes.SingleInstanceAggregationAttribute).ToList();
        ScopedInstance = GetTypesFromAttribute(wellKnownTypes.ScopedInstanceAggregationAttribute).ToList();
        ScopeRoot = GetTypesFromAttribute(wellKnownTypes.ScopeRootAggregationAttribute).ToList();
        Decorator = GetTypesFromAttribute(wellKnownTypes.DecoratorAggregationAttribute).ToList();
            
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
    public IReadOnlyList<INamedTypeSymbol> Transient { get; }
    public IReadOnlyList<INamedTypeSymbol> SingleInstance { get; }
    public IReadOnlyList<INamedTypeSymbol> ScopedInstance { get; }
    public IReadOnlyList<INamedTypeSymbol> ScopeRoot { get; }
    public IReadOnlyList<INamedTypeSymbol> Decorator { get; }
}