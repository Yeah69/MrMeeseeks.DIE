namespace MrMeeseeks.DIE.Utility;

public class NamedTypeSymbolEqualityComparer : IEqualityComparer<INamedTypeSymbol?>
{
    private readonly IEqualityComparer<ISymbol?> _innerEqualityComparer;
    
    public static readonly NamedTypeSymbolEqualityComparer Default = new (SymbolEqualityComparer.Default);
    public static readonly NamedTypeSymbolEqualityComparer IncludeNullability = new (SymbolEqualityComparer.IncludeNullability);

    private NamedTypeSymbolEqualityComparer(IEqualityComparer<ISymbol?> innerEqualityComparer) => 
        _innerEqualityComparer = innerEqualityComparer;

    public bool Equals(INamedTypeSymbol? x, INamedTypeSymbol? y) => 
        _innerEqualityComparer.Equals(x, y);

    public int GetHashCode(INamedTypeSymbol? obj) => 
        _innerEqualityComparer.GetHashCode(obj);
}