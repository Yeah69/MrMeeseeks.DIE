using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal sealed record WellKnownTypes(
    // ReSharper disable InconsistentNaming
    INamedTypeSymbol IDisposable, // .NET Standard 2.0
    INamedTypeSymbol? IAsyncDisposable, // .NET Standard 2.1
    // ReSharper restore InconsistentNaming
    INamedTypeSymbol Lazy1, // .NET Standard 2.0
    INamedTypeSymbol ThreadLocal1, // .NET Standard 2.0
    INamedTypeSymbol? ValueTask, // .NET Standard 2.1
    INamedTypeSymbol? ValueTask1, // .NET Standard 2.1
    INamedTypeSymbol Task, // .NET Standard 2.0
    INamedTypeSymbol Task1, // .NET Standard 2.0
    INamedTypeSymbol ObjectDisposedException, // .NET Standard 2.0
    INamedTypeSymbol ConcurrentBagOfSyncDisposable, // .NET Standard 2.0
    INamedTypeSymbol? ConcurrentBagOfAsyncDisposable, // .NET Standard 2.1
    INamedTypeSymbol ConcurrentDictionaryOfSyncDisposable, // .NET Standard 2.0
    INamedTypeSymbol? ConcurrentDictionaryOfAsyncDisposable,  // .NET Standard 2.1
    INamedTypeSymbol ConcurrentDictionaryOfRuntimeTypeHandleToObject, // .NET Standard 2.0
    INamedTypeSymbol Exception, // .NET Standard 2.0
    INamedTypeSymbol AggregateException, // .NET Standard 2.0
    INamedTypeSymbol SemaphoreSlim, // .NET Standard 2.0
    INamedTypeSymbol Nullable1, // .NET Standard 2.0
    INamedTypeSymbol Type, // .NET Standard 2.0
    INamedTypeSymbol Object) // .NET Standard 2.0
{
    internal static WellKnownTypes Create(Compilation compilation)
    {
        var iDisposable = compilation.GetTypeByMetadataNameOrThrow("System.IDisposable");
        var iAsyncDisposable = compilation.GetTypeByMetadataName("System.IAsyncDisposable");
        var concurrentBag = compilation.GetTypeByMetadataNameOrThrow("System.Collections.Concurrent.ConcurrentBag`1");
        var concurrentDictionary2= compilation.GetTypeByMetadataNameOrThrow("System.Collections.Concurrent.ConcurrentDictionary`2");
        var runtimeTypeHandle = compilation.GetTypeByMetadataNameOrThrow("System.RuntimeTypeHandle");
        var @object = compilation.GetTypeByMetadataNameOrThrow("System.Object");
        
        return new WellKnownTypes(
            IDisposable: iDisposable,
            IAsyncDisposable: iAsyncDisposable,
            Lazy1: compilation.GetTypeByMetadataNameOrThrow("System.Lazy`1"),
            ThreadLocal1: compilation.GetTypeByMetadataNameOrThrow("System.Threading.ThreadLocal`1"),
            ValueTask: compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask"),
            ValueTask1: compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1"),
            Task: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.Task"),
            Task1: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.Task`1"),
            ObjectDisposedException: compilation.GetTypeByMetadataNameOrThrow("System.ObjectDisposedException"),
            ConcurrentBagOfSyncDisposable: concurrentBag.Construct(iDisposable),
            ConcurrentBagOfAsyncDisposable: iAsyncDisposable is not null ? concurrentBag.Construct(iAsyncDisposable) : null,
            ConcurrentDictionaryOfSyncDisposable: concurrentDictionary2.Construct(iDisposable, iDisposable),
            ConcurrentDictionaryOfAsyncDisposable: iAsyncDisposable is not null ? concurrentDictionary2.Construct(iAsyncDisposable, iAsyncDisposable) : null,
            ConcurrentDictionaryOfRuntimeTypeHandleToObject: concurrentDictionary2.Construct(runtimeTypeHandle, @object),
            Exception: compilation.GetTypeByMetadataNameOrThrow("System.Exception"),
            AggregateException: compilation.GetTypeByMetadataNameOrThrow("System.AggregateException"),
            SemaphoreSlim: compilation.GetTypeByMetadataNameOrThrow("System.Threading.SemaphoreSlim"),
            Nullable1: compilation.GetTypeByMetadataNameOrThrow("System.Nullable`1"),
            Type: compilation.GetTypeByMetadataNameOrThrow("System.Type"),
            Object: @object);
    }
}