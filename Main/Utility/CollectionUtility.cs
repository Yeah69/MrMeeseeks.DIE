namespace MrMeeseeks.DIE.Utility;

internal static class CollectionUtility
{
    internal static ITypeSymbol GetCollectionsInnerType(ITypeSymbol type) => type switch
    {
        IArrayTypeSymbol arrayTypeSymbol => arrayTypeSymbol.ElementType,
        INamedTypeSymbol { TypeArguments.Length: 1 } collectionType => collectionType.TypeArguments.First(),
        _ => throw new ArgumentException("Given type is not supported collection type")
    };
}