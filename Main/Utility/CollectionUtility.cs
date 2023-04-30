namespace MrMeeseeks.DIE.Utility;

internal static class CollectionUtility
{
    internal static ITypeSymbol GetCollectionsInnerType(ITypeSymbol type) => type is IArrayTypeSymbol arrayTypeSymbol
        ? arrayTypeSymbol.ElementType
        : type is INamedTypeSymbol { TypeArguments.Length: 1 } collectionType
            ? collectionType.TypeArguments.First()
            : throw new ArgumentException("Given type is not supported collection type");
}