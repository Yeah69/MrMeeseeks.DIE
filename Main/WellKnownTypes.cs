using MrMeeseeks.DIE.Extensions;

namespace MrMeeseeks.DIE;

internal record WellKnownTypes(
    INamedTypeSymbol Disposable,
    INamedTypeSymbol AsyncDisposable,
    INamedTypeSymbol Lazy1,
    INamedTypeSymbol ValueTask,
    INamedTypeSymbol ValueTask1,
    INamedTypeSymbol Task,
    INamedTypeSymbol Task1,
    INamedTypeSymbol ObjectDisposedException,
    INamedTypeSymbol Enumerable1,
    INamedTypeSymbol ReadOnlyCollection1,
    INamedTypeSymbol ReadOnlyList1,
    INamedTypeSymbol ConcurrentBagOfSyncDisposable,
    INamedTypeSymbol ConcurrentBagOfAsyncDisposable,
    INamedTypeSymbol ConcurrentDictionaryOfSyncDisposable,
    INamedTypeSymbol ConcurrentDictionaryOfAsyncDisposable,
    INamedTypeSymbol Exception,
    INamedTypeSymbol TaskCanceledException,
    INamedTypeSymbol SemaphoreSlim,
    INamedTypeSymbol InternalsVisibleToAttribute)
{
    internal static WellKnownTypes Create(Compilation compilation)
    {
        var iDisposable = compilation.GetTypeByMetadataNameOrThrow("System.IDisposable");
        var iAsyncDisposable = compilation.GetTypeByMetadataNameOrThrow("System.IAsyncDisposable");
        var concurrentBag = compilation.GetTypeByMetadataNameOrThrow("System.Collections.Concurrent.ConcurrentBag`1");
        var concurrentDictionary2= compilation.GetTypeByMetadataNameOrThrow("System.Collections.Concurrent.ConcurrentDictionary`2");

        return new WellKnownTypes(
                Disposable: compilation.GetTypeByMetadataNameOrThrow("System.IDisposable"),
                AsyncDisposable: compilation.GetTypeByMetadataNameOrThrow("System.IAsyncDisposable"),
                Lazy1: compilation.GetTypeByMetadataNameOrThrow("System.Lazy`1"),
                ValueTask: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.ValueTask"),
                ValueTask1: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.ValueTask`1"),
                Task: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.Task"),
                Task1: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.Task`1"),
                ObjectDisposedException: compilation.GetTypeByMetadataNameOrThrow("System.ObjectDisposedException"),
                Enumerable1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.IEnumerable`1"),
                ReadOnlyCollection1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.IReadOnlyCollection`1"),
                ReadOnlyList1: compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.IReadOnlyList`1"),
                ConcurrentBagOfSyncDisposable: concurrentBag.Construct(iDisposable),
                ConcurrentBagOfAsyncDisposable: concurrentBag.Construct(iAsyncDisposable),
                ConcurrentDictionaryOfSyncDisposable: concurrentDictionary2.Construct(iDisposable, iDisposable),
                ConcurrentDictionaryOfAsyncDisposable: concurrentDictionary2.Construct(iAsyncDisposable, iAsyncDisposable),
                Exception: compilation.GetTypeByMetadataNameOrThrow("System.Exception"),
                TaskCanceledException: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.TaskCanceledException"),
                SemaphoreSlim: compilation.GetTypeByMetadataNameOrThrow("System.Threading.SemaphoreSlim"),
                InternalsVisibleToAttribute: compilation.GetTypeByMetadataNameOrThrow("System.Runtime.CompilerServices.InternalsVisibleToAttribute"));
    }
}