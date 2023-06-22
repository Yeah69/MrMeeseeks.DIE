using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal record WellKnownTypes(
    // ReSharper disable InconsistentNaming
    INamedTypeSymbol IDisposable,
    INamedTypeSymbol IAsyncDisposable,
    // ReSharper restore InconsistentNaming
    INamedTypeSymbol Lazy1,
    INamedTypeSymbol ValueTask,
    INamedTypeSymbol ValueTask1,
    INamedTypeSymbol Task,
    INamedTypeSymbol Task1,
    INamedTypeSymbol ObjectDisposedException,
    INamedTypeSymbol ConcurrentBagOfSyncDisposable,
    INamedTypeSymbol ConcurrentBagOfAsyncDisposable,
    INamedTypeSymbol ConcurrentDictionaryOfSyncDisposable,
    INamedTypeSymbol ConcurrentDictionaryOfAsyncDisposable,
    INamedTypeSymbol Exception,
    INamedTypeSymbol AggregateException,
    INamedTypeSymbol SemaphoreSlim,
    INamedTypeSymbol InternalsVisibleToAttribute,
    INamedTypeSymbol Nullable1)
{
    internal static WellKnownTypes Create(Compilation compilation)
    {
        var iDisposable = compilation.GetTypeByMetadataNameOrThrow("System.IDisposable");
        var iAsyncDisposable = compilation.GetTypeByMetadataNameOrThrow("System.IAsyncDisposable");
        var concurrentBag = compilation.GetTypeByMetadataNameOrThrow("System.Collections.Concurrent.ConcurrentBag`1");
        var concurrentDictionary2= compilation.GetTypeByMetadataNameOrThrow("System.Collections.Concurrent.ConcurrentDictionary`2");

        return new WellKnownTypes(
            IDisposable: iDisposable,
            IAsyncDisposable: iAsyncDisposable,
            Lazy1: compilation.GetTypeByMetadataNameOrThrow("System.Lazy`1"),
            ValueTask: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.ValueTask"),
            ValueTask1: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.ValueTask`1"),
            Task: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.Task"),
            Task1: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.Task`1"),
            ObjectDisposedException: compilation.GetTypeByMetadataNameOrThrow("System.ObjectDisposedException"),
            ConcurrentBagOfSyncDisposable: concurrentBag.Construct(iDisposable),
            ConcurrentBagOfAsyncDisposable: concurrentBag.Construct(iAsyncDisposable),
            ConcurrentDictionaryOfSyncDisposable: concurrentDictionary2.Construct(iDisposable, iDisposable),
            ConcurrentDictionaryOfAsyncDisposable: concurrentDictionary2.Construct(iAsyncDisposable, iAsyncDisposable),
            Exception: compilation.GetTypeByMetadataNameOrThrow("System.Exception"),
            AggregateException: compilation.GetTypeByMetadataNameOrThrow("System.AggregateException"),
            SemaphoreSlim: compilation.GetTypeByMetadataNameOrThrow("System.Threading.SemaphoreSlim"),
            InternalsVisibleToAttribute: compilation.GetTypeByMetadataNameOrThrow("System.Runtime.CompilerServices.InternalsVisibleToAttribute"),
            Nullable1: compilation.GetTypeByMetadataNameOrThrow("System.Nullable`1"));
    }
}