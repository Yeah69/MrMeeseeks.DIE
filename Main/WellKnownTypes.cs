using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE
{
    internal record WellKnownTypes(
        INamedTypeSymbol Container,
        INamedTypeSymbol SpyAttribute,
        INamedTypeSymbol TransientAttribute,
        INamedTypeSymbol SingleInstanceAttribute,
        INamedTypeSymbol Disposable,
        INamedTypeSymbol AsyncDisposable,
        INamedTypeSymbol Lazy1,
        INamedTypeSymbol ValueTask,
        INamedTypeSymbol ValueTask1,
        INamedTypeSymbol Task1,
        INamedTypeSymbol ObjectDisposedException,
        INamedTypeSymbol Enumerable1,
        INamedTypeSymbol ReadOnlyCollection1,
        INamedTypeSymbol ReadOnlyList1,
        INamedTypeSymbol ConcurrentBagOfDisposable,
        INamedTypeSymbol Action,
        INamedTypeSymbol Func,
        INamedTypeSymbol Exception,
        INamedTypeSymbol SemaphoreSlim)
    {
        public static bool TryCreate(Compilation compilation, out WellKnownTypes wellKnownTypes)
        {
            var iContainer = compilation.GetTypeOrReport("MrMeeseeks.DIE.IContainer`1");
            var iDisposable = compilation.GetTypeOrReport("System.IDisposable");
            var iAsyncDisposable = compilation.GetTypeOrReport("System.IAsyncDisposable");
            var lazy1 = compilation.GetTypeOrReport("System.Lazy`1");
            var valueTask = compilation.GetTypeOrReport("System.Threading.Tasks.ValueTask");
            var valueTask1 = compilation.GetTypeOrReport("System.Threading.Tasks.ValueTask`1");
            var task1 = compilation.GetTypeOrReport("System.Threading.Tasks.Task`1");
            var objectDisposedException = compilation.GetTypeOrReport("System.ObjectDisposedException");
            var iEnumerable1 = compilation.GetTypeOrReport("System.Collections.Generic.IEnumerable`1");
            var iReadOnlyCollection1 = compilation.GetTypeOrReport("System.Collections.Generic.IReadOnlyCollection`1");
            var iReadOnlyList1 = compilation.GetTypeOrReport("System.Collections.Generic.IReadOnlyList`1");
            var concurrentBag = compilation.GetTypeOrReport("System.Collections.Concurrent.ConcurrentBag`1");
            var concurrentBagOfDisposable = iDisposable is null
                ? null
                : concurrentBag?.Construct(iDisposable);
            var action = compilation.GetTypeOrReport("System.Action");
            var func = compilation.GetTypeOrReport("System.Func`3");
            var exception = compilation.GetTypeOrReport("System.Exception");
            var semaphoreSlim = compilation.GetTypeOrReport("System.Threading.SemaphoreSlim");

            var spyAttribute = compilation
                .GetTypeByMetadataName(typeof(SpyAttribute).FullName ?? "");

            var transientAttribute = compilation
                .GetTypeByMetadataName(typeof(TransientAttribute).FullName ?? "");

            var singleInstanceAttribute = compilation
                .GetTypeByMetadataName(typeof(SingleInstanceAttribute).FullName ?? "");

            if (iContainer is null
                || spyAttribute is null
                || transientAttribute is null
                || singleInstanceAttribute is null
                || iDisposable is null
                || iAsyncDisposable is null
                || lazy1 is null
                || valueTask is null
                || valueTask1 is null
                || task1 is null
                || objectDisposedException is null
                || iEnumerable1 is null
                || iReadOnlyCollection1 is null
                || iReadOnlyList1 is null
                || concurrentBagOfDisposable is null
                || action is null
                || func is null
                || exception is null
                || semaphoreSlim is null)
            {
                wellKnownTypes = null!;
                return false;
            }

            wellKnownTypes = new WellKnownTypes(
                Container: iContainer,
                SpyAttribute: spyAttribute,
                TransientAttribute: transientAttribute,
                SingleInstanceAttribute: singleInstanceAttribute,
                Disposable: iDisposable,
                AsyncDisposable: iAsyncDisposable,
                Lazy1: lazy1,
                ValueTask: valueTask,
                ValueTask1: valueTask1,
                Task1: task1,
                ObjectDisposedException: objectDisposedException,
                Enumerable1: iEnumerable1,
                ReadOnlyCollection1: iReadOnlyCollection1,
                ReadOnlyList1: iReadOnlyList1,
                ConcurrentBagOfDisposable: concurrentBagOfDisposable,
                Action: action,
                Func: func,
                Exception: exception,
                SemaphoreSlim: semaphoreSlim);

            return true;
        }
    }
}