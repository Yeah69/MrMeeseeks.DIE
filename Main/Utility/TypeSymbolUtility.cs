using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Utility;

internal class TypeSymbolUtility(WellKnownTypes wellKnownTypes) : IContainerInstance
{
    internal ITypeSymbol GetUnwrappedType(ITypeSymbol type)
    {
        if (IsWrapTypeOfSingleGenericType(type)
            && type is INamedTypeSymbol namedType)
            return GetUnwrappedType(namedType.TypeArguments.First());

        if (IsFuncDelegate(type) && type is INamedTypeSymbol func)
            return GetUnwrappedType(func.TypeArguments.Last());

        return type;
    }
    internal bool IsWrapType(ITypeSymbol type) =>
        IsWrapTypeOfSingleGenericType(type) || IsFuncDelegate(type);

    internal bool IsFuncDelegate(ITypeSymbol type) =>
        type.TypeKind == TypeKind.Delegate && type.FullName().StartsWith("global::System.Func<", StringComparison.Ordinal);

    internal bool IsWrapTypeOfSingleGenericType(ITypeSymbol type) =>
        wellKnownTypes.ValueTask1 is not null && CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.ValueTask1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Task1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.Lazy1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, wellKnownTypes.ThreadLocal1);
}