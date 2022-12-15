namespace MrMeeseeks.DIE.Utility;

/*public sealed class CustomSymbolEqualityComparer : IEqualityComparer<ISymbol?>
{
    private readonly bool _considerNullability;
    public static readonly SymbolEqualityComparer Default = new (false);

    public static readonly SymbolEqualityComparer IncludeNullability = new (true);

    internal CustomSymbolEqualityComparer(bool considerNullability) => _considerNullability = considerNullability;
    
    public bool Equals(ISymbol? x, ISymbol? y)
    {
        if (x is null) return y is null;

        if (y is null 
            || !Equals(x.ContainingNamespace?.ToDisplayString(), y.ContainingNamespace?.ToDisplayString())
            || !x.Name.Equals(y.Name))
            return false;

        if (x is IArrayTypeSymbol xArray && y is IArrayTypeSymbol yArray)
            return Equals(xArray.ElementType, yArray.ElementType) 
                   && (!_considerNullability || xArray.NullableAnnotation.Equals(yArray.NullableAnnotation));
        
        if (x is INamedTypeSymbol xNamed && y is INamedTypeSymbol yNamed)
            return xNamed.TypeArguments.Length == yNamed.TypeArguments.Length 
                   && xNamed.TypeArguments.Zip(yNamed.TypeArguments, Equals).All(b => b)
                   && (!_considerNullability || xNamed.NullableAnnotation.Equals(yNamed.NullableAnnotation));
        
        if (x is ITypeSymbol xType && y is ITypeSymbol yType)
            return !_considerNullability || xType.NullableAnnotation.Equals(yType.NullableAnnotation);

        return true;
    }

    public int GetHashCode(ISymbol? obj) =>
        _considerNullability 
            ? SymbolEqualityComparer.IncludeNullability.GetHashCode(obj) 
            : SymbolEqualityComparer.Default.GetHashCode(obj);
}//*/