using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Extensions;

internal static class ITypeSymbolExtensions
{
    internal static TypeKey ToTypeKey(this ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedTypeSymbol)
            return namedTypeSymbol.ConstructTypeUniqueKey();
        return new TypeKey(type.FullName());
    }
}