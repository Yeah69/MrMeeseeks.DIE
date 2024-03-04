using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Utility;

internal interface ICheckIterableTypes
{
    bool IsCollectionType(ITypeSymbol type);
    bool IsMapType(ITypeSymbol type);
    bool MapTypeHasPluralItemType(INamedTypeSymbol mapType);
}

internal sealed class CheckIterableTypes : ICheckIterableTypes
{
    private readonly WellKnownTypesCollections _wellKnownTypesCollections;

    internal CheckIterableTypes(IContainerWideContext containerWideContext)
    {
        _wellKnownTypesCollections = containerWideContext.WellKnownTypesCollections;
    }
    
    public bool IsCollectionType(ITypeSymbol type) =>
        CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IEnumerable1)
        || _wellKnownTypesCollections.IAsyncEnumerable1 is not null 
        && CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IAsyncEnumerable1)
        || type is IArrayTypeSymbol
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IList1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ICollection1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ReadOnlyCollection1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IReadOnlyCollection1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IReadOnlyList1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ArraySegment1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ConcurrentBag1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ConcurrentQueue1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ConcurrentStack1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.HashSet1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.LinkedList1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.List1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.Queue1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.SortedSet1)
        || CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.Stack1)
        || _wellKnownTypesCollections.ImmutableArray1 is not null 
        && CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ImmutableArray1)
        || _wellKnownTypesCollections.ImmutableHashSet1 is not null 
        && CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ImmutableHashSet1)
        || _wellKnownTypesCollections.ImmutableList1 is not null 
        && CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ImmutableList1)
        || _wellKnownTypesCollections.ImmutableQueue1 is not null 
        && CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ImmutableQueue1)
        || _wellKnownTypesCollections.ImmutableSortedSet1 is not null 
        && CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ImmutableSortedSet1)
        || _wellKnownTypesCollections.ImmutableStack1 is not null 
        && CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.ImmutableStack1);

    public bool IsMapType(ITypeSymbol type) =>
        type is INamedTypeSymbol namedType // All map types are named types
        && (IsIterableKeyValueType(namedType)
            || CustomSymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _wellKnownTypesCollections.IDictionary2)
            || CustomSymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _wellKnownTypesCollections.IReadOnlyDictionary2)
            || CustomSymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _wellKnownTypesCollections.Dictionary2)
            || CustomSymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _wellKnownTypesCollections.ReadOnlyDictionary2)
            || CustomSymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _wellKnownTypesCollections.SortedDictionary2)
            || CustomSymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _wellKnownTypesCollections.SortedList2)
            || _wellKnownTypesCollections.ImmutableDictionary2 is not null 
            && CustomSymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _wellKnownTypesCollections.ImmutableDictionary2)
            || _wellKnownTypesCollections.ImmutableSortedDictionary2 is not null 
            && CustomSymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _wellKnownTypesCollections.ImmutableSortedDictionary2));

    public bool MapTypeHasPluralItemType(INamedTypeSymbol mapType)
    {
        var checkedType = IsIterableKeyValueType(mapType) && mapType.TypeArguments[0] is INamedTypeSymbol kvpType
            ? kvpType
            : mapType;
        
        return IsCollectionType(checkedType.TypeArguments[1]);
    }

    private bool IsIterableKeyValueType(INamedTypeSymbol type) =>
        (CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IEnumerable1)
         || _wellKnownTypesCollections.IAsyncEnumerable1 is not null 
         && CustomSymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IAsyncEnumerable1))
        && CustomSymbolEqualityComparer.Default.Equals(type.TypeArguments[0].OriginalDefinition, _wellKnownTypesCollections.KeyValuePair2);
}