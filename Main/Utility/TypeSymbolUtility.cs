using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Utility;

internal static class TypeSymbolUtility
{
    internal static ITypeSymbol GetUnwrappedType(ITypeSymbol type, WellKnownTypes wellKnownTypes)
    {
        if (IsWrapTypeOfSingleGenericType(type, wellKnownTypes)
            && type is INamedTypeSymbol namedType)
            return GetUnwrappedType(namedType.TypeArguments.First(), wellKnownTypes);

        if (IsFuncDelegate(type) && type is INamedTypeSymbol func)
            return GetUnwrappedType(func.TypeArguments.Last(), wellKnownTypes);

        return type;
    }
    internal static bool IsWrapType(ITypeSymbol type, WellKnownTypes wellKnownTypes) =>
        IsWrapTypeOfSingleGenericType(type, wellKnownTypes) || IsFuncDelegate(type);

    internal static bool IsFuncDelegate(ITypeSymbol type) =>
        type.TypeKind == TypeKind.Delegate && type.FullName().StartsWith("global::System.Func<", StringComparison.Ordinal);

    internal static bool IsWrapTypeOfSingleGenericType(ITypeSymbol type, WellKnownTypes wellKnownTypes) =>
        CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.ValueTask1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Task1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Lazy1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.ThreadLocal1);
}