namespace MrMeeseeks.DIE.Extensions;

internal static class ITypeSymbolExtensions
{
    internal static string ConstructTypeUniqueKey(this ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedTypeSymbol)
            return namedTypeSymbol.ConstructTypeUniqueKey();
        return type.FullName();
    }
}