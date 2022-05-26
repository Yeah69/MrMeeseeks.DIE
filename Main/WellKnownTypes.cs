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
    internal static bool TryCreate(Compilation compilation, out WellKnownTypes wellKnownTypes)
    {
        var iDisposable = compilation.GetTypeOrReport("System.IDisposable");
        var iAsyncDisposable = compilation.GetTypeOrReport("System.IAsyncDisposable");
        var lazy1 = compilation.GetTypeOrReport("System.Lazy`1");
        var valueTask = compilation.GetTypeOrReport("System.Threading.Tasks.ValueTask");
        var valueTask1 = compilation.GetTypeOrReport("System.Threading.Tasks.ValueTask`1");
        var task = compilation.GetTypeOrReport("System.Threading.Tasks.Task");
        var task1 = compilation.GetTypeOrReport("System.Threading.Tasks.Task`1");
        var objectDisposedException = compilation.GetTypeOrReport("System.ObjectDisposedException");
        var iEnumerable1 = compilation.GetTypeOrReport("System.Collections.Generic.IEnumerable`1");
        var iReadOnlyCollection1 = compilation.GetTypeOrReport("System.Collections.Generic.IReadOnlyCollection`1");
        var iReadOnlyList1 = compilation.GetTypeOrReport("System.Collections.Generic.IReadOnlyList`1");
        var concurrentBag = compilation.GetTypeOrReport("System.Collections.Concurrent.ConcurrentBag`1");
        var concurrentBagOfSyncDisposable = iDisposable is null
            ? null
            : concurrentBag?.Construct(iDisposable);
        var concurrentBagOfAsyncDisposable = iAsyncDisposable is null
            ? null
            : concurrentBag?.Construct(iAsyncDisposable);
        var concurrentDictionary2= compilation.GetTypeOrReport("System.Collections.Concurrent.ConcurrentDictionary`2");
        var concurrentDictionary2OfSyncDisposable = iDisposable is null
            ? null
            : concurrentDictionary2?.Construct(iDisposable, iDisposable);
        var concurrentDictionary2OfAsyncDisposable = iAsyncDisposable is null
            ? null
            : concurrentDictionary2?.Construct(iAsyncDisposable, iAsyncDisposable);
        var exception = compilation.GetTypeOrReport("System.Exception");
        var taskCanceledException = compilation.GetTypeOrReport("System.Threading.Tasks.TaskCanceledException");
        var semaphoreSlim = compilation.GetTypeOrReport("System.Threading.SemaphoreSlim");
        var internalsVisibleToAttribute = compilation.GetTypeOrReport("System.Runtime.CompilerServices.InternalsVisibleToAttribute");

        if (iDisposable is not null
            && iAsyncDisposable is not null
            && lazy1 is not null
            && valueTask is not null
            && task is not null
            && valueTask1 is not null
            && task1 is not null
            && taskCanceledException is not null
            && objectDisposedException is not null
            && iEnumerable1 is not null
            && iReadOnlyCollection1 is not null
            && iReadOnlyList1 is not null
            && concurrentBagOfSyncDisposable is not null
            && concurrentBagOfAsyncDisposable is not null
            && concurrentDictionary2OfSyncDisposable is not null
            && concurrentDictionary2OfAsyncDisposable is not null
            && exception is not null
            && semaphoreSlim is not null
            && internalsVisibleToAttribute is not null)
        {

            wellKnownTypes = new WellKnownTypes(
                Disposable: iDisposable,
                AsyncDisposable: iAsyncDisposable,
                Lazy1: lazy1,
                ValueTask: valueTask,
                ValueTask1: valueTask1,
                Task: task,
                Task1: task1,
                ObjectDisposedException: objectDisposedException,
                Enumerable1: iEnumerable1,
                ReadOnlyCollection1: iReadOnlyCollection1,
                ReadOnlyList1: iReadOnlyList1,
                ConcurrentBagOfSyncDisposable: concurrentBagOfSyncDisposable,
                ConcurrentBagOfAsyncDisposable: concurrentBagOfAsyncDisposable,
                ConcurrentDictionaryOfSyncDisposable: concurrentDictionary2OfSyncDisposable,
                ConcurrentDictionaryOfAsyncDisposable: concurrentDictionary2OfAsyncDisposable,
                Exception: exception,
                TaskCanceledException: taskCanceledException,
                SemaphoreSlim: semaphoreSlim,
                InternalsVisibleToAttribute: internalsVisibleToAttribute);
            return true;
        }
        
        wellKnownTypes = null!;
        return false;
    }
}