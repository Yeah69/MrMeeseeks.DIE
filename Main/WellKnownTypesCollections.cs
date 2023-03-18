using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal record WellKnownTypesCollections(
    // ReSharper disable InconsistentNaming
    INamedTypeSymbol IEnumerable1,
    INamedTypeSymbol IAsyncEnumerable1,
    // ReSharper restore InconsistentNaming
    INamedTypeSymbol ArraySegment1,
    INamedTypeSymbol Enumerable,
    // ReSharper disable InconsistentNaming
    INamedTypeSymbol IList1,
    INamedTypeSymbol ICollection1,
    INamedTypeSymbol IReadOnlyCollection1,
    // ReSharper restore InconsistentNaming
    INamedTypeSymbol ReadOnlyCollection1,
    // ReSharper disable once InconsistentNaming
    INamedTypeSymbol IReadOnlyList1,
    INamedTypeSymbol ConcurrentBag1,
    INamedTypeSymbol ConcurrentQueue1,
    INamedTypeSymbol ConcurrentStack1,
    INamedTypeSymbol HashSet1,
    INamedTypeSymbol LinkedList1,
    INamedTypeSymbol List1,
    INamedTypeSymbol Queue1,
    INamedTypeSymbol SortedSet1,
    INamedTypeSymbol Stack1,
    INamedTypeSymbol ImmutableArray1,
    INamedTypeSymbol ImmutableArray,
    INamedTypeSymbol ImmutableHashSet1,
    INamedTypeSymbol ImmutableHashSet,
    INamedTypeSymbol ImmutableList1,
    INamedTypeSymbol ImmutableList,
    INamedTypeSymbol ImmutableQueue1,
    INamedTypeSymbol ImmutableQueue,
    INamedTypeSymbol ImmutableSortedSet1,
    INamedTypeSymbol ImmutableSortedSet,
    INamedTypeSymbol ImmutableStack1,
    INamedTypeSymbol ImmutableStack
    )
{
    internal static WellKnownTypesCollections Create(Compilation compilation)
    {
        return new WellKnownTypesCollections(
            IEnumerable1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.IEnumerable`1"),
            IAsyncEnumerable1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.IAsyncEnumerable`1"),
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
            ImmutableArray1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Immutable.ImmutableArray`1"),
            ImmutableArray: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Immutable.ImmutableArray"),
            ImmutableHashSet1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Immutable.ImmutableHashSet`1"),
            ImmutableHashSet: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Immutable.ImmutableHashSet"),
            ImmutableList1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Immutable.ImmutableList`1"),
            ImmutableList: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Immutable.ImmutableList"),
            ImmutableQueue1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Immutable.ImmutableQueue`1"),
            ImmutableQueue: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Immutable.ImmutableQueue"),
            ImmutableSortedSet1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Immutable.ImmutableSortedSet`1"),
            ImmutableSortedSet: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Immutable.ImmutableSortedSet"),
            ImmutableStack1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Immutable.ImmutableStack`1"),
            ImmutableStack: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Immutable.ImmutableStack"));
    }
}