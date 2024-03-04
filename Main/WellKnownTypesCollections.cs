using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal sealed record WellKnownTypesCollections(
    // ReSharper disable InconsistentNaming
    INamedTypeSymbol IEnumerable1, // .NET Standard 2.0
    INamedTypeSymbol? IAsyncEnumerable1, // .NET Standard 2.1
    // ReSharper restore InconsistentNaming
    INamedTypeSymbol ArraySegment1, // .NET Standard 2.0
    INamedTypeSymbol Enumerable, // .NET Standard 2.0
    // ReSharper disable InconsistentNaming
    INamedTypeSymbol IList1, // .NET Standard 2.0
    INamedTypeSymbol ICollection1, // .NET Standard 2.0
    INamedTypeSymbol IReadOnlyCollection1, // .NET Standard 2.0
    // ReSharper restore InconsistentNaming
    INamedTypeSymbol ReadOnlyCollection1, // .NET Standard 2.0
    // ReSharper disable once InconsistentNaming
    INamedTypeSymbol IReadOnlyList1, // .NET Standard 2.0
    INamedTypeSymbol ConcurrentBag1, // .NET Standard 2.0
    INamedTypeSymbol ConcurrentQueue1, // .NET Standard 2.0
    INamedTypeSymbol ConcurrentStack1, // .NET Standard 2.0
    INamedTypeSymbol HashSet1, // .NET Standard 2.0
    INamedTypeSymbol LinkedList1, // .NET Standard 2.0
    INamedTypeSymbol List1, // .NET Standard 2.0
    INamedTypeSymbol Queue1, // .NET Standard 2.0
    INamedTypeSymbol SortedSet1, // .NET Standard 2.0
    INamedTypeSymbol Stack1, // .NET Standard 2.0
    INamedTypeSymbol? ImmutableArray1, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableArray, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableHashSet1, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableHashSet, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableList1, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableList, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableQueue1, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableQueue, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableSortedSet1, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableSortedSet, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableStack1, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableStack, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol KeyValuePair2, // .NET Standard 2.0
    INamedTypeSymbol IDictionary2, // .NET Standard 2.0
    INamedTypeSymbol IReadOnlyDictionary2, // .NET Standard 2.0
    INamedTypeSymbol Dictionary2, // .NET Standard 2.0
    INamedTypeSymbol ReadOnlyDictionary2, // .NET Standard 2.0
    INamedTypeSymbol SortedDictionary2, // .NET Standard 2.0
    INamedTypeSymbol SortedList2, // .NET Standard 2.0
    INamedTypeSymbol? ImmutableDictionary2, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableDictionary, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableSortedDictionary2, // None (nuget: System.Collections.Immutable)
    INamedTypeSymbol? ImmutableSortedDictionary) // None (nuget: System.Collections.Immutable)
{
    internal static WellKnownTypesCollections Create(Compilation compilation)
    {
        return new WellKnownTypesCollections(
            IEnumerable1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.IEnumerable`1"),
            IAsyncEnumerable1: compilation.GetTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1"),
            ArraySegment1: compilation.GetTypeByMetadataNameOrThrow("System.ArraySegment`1"),
            Enumerable: compilation.GetTypeByMetadataNameOrThrow("System.Linq.Enumerable"),
            IList1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.IList`1"),
            ICollection1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.ICollection`1"),
            IReadOnlyCollection1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.IReadOnlyCollection`1"),
            ReadOnlyCollection1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.ObjectModel.ReadOnlyCollection`1"),
            IReadOnlyList1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.IReadOnlyList`1"),
            ConcurrentBag1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Concurrent.ConcurrentBag`1"),
            ConcurrentQueue1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Concurrent.ConcurrentQueue`1"),
            ConcurrentStack1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Concurrent.ConcurrentStack`1"),
            HashSet1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.HashSet`1"),
            LinkedList1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.LinkedList`1"),
            List1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.List`1"),
            Queue1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.Queue`1"),
            SortedSet1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.SortedSet`1"),
            Stack1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.Stack`1"),
            ImmutableArray1: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1"),
            ImmutableArray: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray"),
            ImmutableHashSet1: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableHashSet`1"),
            ImmutableHashSet: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableHashSet"),
            ImmutableList1: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableList`1"),
            ImmutableList: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableList"),
            ImmutableQueue1: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableQueue`1"),
            ImmutableQueue: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableQueue"),
            ImmutableSortedSet1: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableSortedSet`1"),
            ImmutableSortedSet: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableSortedSet"),
            ImmutableStack1: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableStack`1"),
            ImmutableStack: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableStack"),
            KeyValuePair2: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.KeyValuePair`2"),
            IDictionary2: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.IDictionary`2"),
            IReadOnlyDictionary2: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.IReadOnlyDictionary`2"),
            Dictionary2: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.Dictionary`2"),
            ReadOnlyDictionary2: compilation.GetTypeByMetadataNameOrThrow("System.Collections.ObjectModel.ReadOnlyDictionary`2"),
            SortedDictionary2: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.SortedDictionary`2"),
            SortedList2: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.SortedList`2"),
            ImmutableDictionary2: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableDictionary`2"),
            ImmutableDictionary: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableDictionary"),
            ImmutableSortedDictionary2: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableSortedDictionary`2"),
            ImmutableSortedDictionary: compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableSortedDictionary"));
    }
}