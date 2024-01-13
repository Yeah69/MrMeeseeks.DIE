namespace MrMeeseeks.DIE.Extensions;

public static class ITypeSymbolExtensions
{
    public static IEnumerable<ITypeParameterSymbol> ExtractTypeParameters(this ITypeSymbol typeSymbol) =>
        typeSymbol switch
        {
            ITypeParameterSymbol typeParameterSymbol => new[] { typeParameterSymbol },
            INamedTypeSymbol namedTypeSymbol => namedTypeSymbol.TypeParameters,
            IArrayTypeSymbol => Enumerable.Empty<ITypeParameterSymbol>(),
            IPointerTypeSymbol => Enumerable.Empty<ITypeParameterSymbol>(),
            IFunctionPointerTypeSymbol functionPointerTypeSymbol => functionPointerTypeSymbol
                .Signature
                .TypeParameters,
            _ => Enumerable.Empty<ITypeParameterSymbol>()
        };
}