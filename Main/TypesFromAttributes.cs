namespace MrMeeseeks.DIE;

public interface ITypesFromAttributes
{
    IReadOnlyList<INamedTypeSymbol> Spy { get; }
    IReadOnlyList<INamedTypeSymbol> Transient { get; }
    IReadOnlyList<INamedTypeSymbol> SingleInstance { get; }
    IReadOnlyList<INamedTypeSymbol> ScopedInstance { get; }
    IReadOnlyList<INamedTypeSymbol> ScopeRoot { get; }
}

internal class TypesFromAttributes : ITypesFromAttributes
{
    public TypesFromAttributes(
        WellKnownTypes wellKnownTypes,
        IGetAssemblyAttributes getAssemblyAttributes)
    {
        Spy = GetTypesFromAttribute(wellKnownTypes.SpyAttribute).ToList();
        Transient = GetTypesFromAttribute(wellKnownTypes.TransientAttribute).ToList();
        SingleInstance = GetTypesFromAttribute(wellKnownTypes.SingleInstanceAttribute).ToList();
        ScopedInstance = GetTypesFromAttribute(wellKnownTypes.ScopedInstanceAttribute).ToList();
        ScopeRoot = GetTypesFromAttribute(wellKnownTypes.ScopeRootAttribute).ToList();
            
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
            .OfType<INamedTypeSymbol>();

        bool CheckValidType(TypedConstant typedConstant, out INamedTypeSymbol type)
        {
            type = (typedConstant.Value as INamedTypeSymbol)!;
            if (typedConstant.Value is null)
                return false;
            if (type.IsUnboundGenericType)
                return false;

            return true;
        }
    }
        
    public IReadOnlyList<INamedTypeSymbol> Spy { get; }
    public IReadOnlyList<INamedTypeSymbol> Transient { get; }
    public IReadOnlyList<INamedTypeSymbol> SingleInstance { get; }
    public IReadOnlyList<INamedTypeSymbol> ScopedInstance { get; }
    public IReadOnlyList<INamedTypeSymbol> ScopeRoot { get; }
}