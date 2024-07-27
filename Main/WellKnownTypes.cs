using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal sealed record WellKnownTypes(
    // ReSharper disable InconsistentNaming
    INamedTypeSymbol IDisposable, // .NET Standard 2.0
    INamedTypeSymbol? IAsyncDisposable, // .NET Standard 2.1
    // ReSharper restore InconsistentNaming
    INamedTypeSymbol Lazy1, // .NET Standard 2.0
    INamedTypeSymbol Func1, // .NET Standard 2.0
    INamedTypeSymbol Func2, // .NET Standard 2.0
    INamedTypeSymbol Func3, // .NET Standard 2.0
    INamedTypeSymbol Func4, // .NET Standard 2.0
    INamedTypeSymbol Func5, // .NET Standard 2.0
    INamedTypeSymbol Func6, // .NET Standard 2.0
    INamedTypeSymbol Func7, // .NET Standard 2.0
    INamedTypeSymbol Func8, // .NET Standard 2.0
    INamedTypeSymbol Func9, // .NET Standard 2.0
    INamedTypeSymbol Func10, // .NET Standard 2.0
    INamedTypeSymbol Func11, // .NET Standard 2.0
    INamedTypeSymbol Func12, // .NET Standard 2.0
    INamedTypeSymbol Func13, // .NET Standard 2.0
    INamedTypeSymbol Func14, // .NET Standard 2.0
    INamedTypeSymbol Func15, // .NET Standard 2.0
    INamedTypeSymbol Func16, // .NET Standard 2.0
    INamedTypeSymbol Func17, // .NET Standard 2.0
    INamedTypeSymbol ThreadLocal1, // .NET Standard 2.0
    INamedTypeSymbol? ValueTask, // .NET Standard 2.1
    INamedTypeSymbol? ValueTask1, // .NET Standard 2.1
    INamedTypeSymbol Task, // .NET Standard 2.0
    INamedTypeSymbol Task1, // .NET Standard 2.0
    INamedTypeSymbol TaskCompletionSource1, // .NET Standard 2.0
    INamedTypeSymbol TaskCompletionSourceOfInt, // .NET Standard 2.0
    INamedTypeSymbol SpinWait, // .NET Standard 2.0
    INamedTypeSymbol Thread, // .NET Standard 2.0
    INamedTypeSymbol Action, // .NET Standard 2.0
    INamedTypeSymbol ObjectDisposedException, // .NET Standard 2.0
    INamedTypeSymbol ConcurrentBagOfSyncDisposable, // .NET Standard 2.0
    INamedTypeSymbol? ConcurrentBagOfAsyncDisposable, // .NET Standard 2.1
    INamedTypeSymbol ConcurrentDictionaryOfSyncDisposable, // .NET Standard 2.0
    INamedTypeSymbol? ConcurrentDictionaryOfAsyncDisposable,  // .NET Standard 2.1
    INamedTypeSymbol ListOfObject, // .NET Standard 2.0
    INamedTypeSymbol ListOfListOfObject, // .NET Standard 2.0
    INamedTypeSymbol ConcurrentStackOfObject, // .NET Standard 2.0
    INamedTypeSymbol ConcurrentStackOfConcurrentStackOfObject, // .NET Standard 2.0
    INamedTypeSymbol ConcurrentDictionaryOfRuntimeTypeHandleToObject, // .NET Standard 2.0
    INamedTypeSymbol Exception, // .NET Standard 2.0
    // ReSharper disable InconsistentNaming
    INamedTypeSymbol IEnumerableOfException, // .NET Standard 2.0
    INamedTypeSymbol? IAsyncEnumerableOfException, // .NET Standard 2.1
    // ReSharper restore InconsistentNaming
    INamedTypeSymbol ListOfException, // .NET Standard 2.0
    INamedTypeSymbol AggregateException, // .NET Standard 2.0
    INamedTypeSymbol TaskOfException, // .NET Standard 2.0
    INamedTypeSymbol TaskOfNullableAggregateException, // .NET Standard 2.0
    INamedTypeSymbol SemaphoreSlim, // .NET Standard 2.0
    INamedTypeSymbol Int32, // .NET Standard 2.0
    INamedTypeSymbol Nullable1, // .NET Standard 2.0
    INamedTypeSymbol Interlocked, // .NET Standard 2.0
    INamedTypeSymbol Type, // .NET Standard 2.0
    INamedTypeSymbol MethodInfo, // .NET Standard 2.0
    INamedTypeSymbol String, // .NET Standard 2.0
    INamedTypeSymbol Object, // .NET Standard 2.0
    INamedTypeSymbol Boolean) // .NET Standard 2.0
    : IContainerInstance
{
    internal static WellKnownTypes Create(Compilation compilation)
    {
        var iDisposable = compilation.GetTypeByMetadataNameOrThrow("System.IDisposable");
        var iAsyncDisposable = compilation.GetTypeByMetadataName("System.IAsyncDisposable");
        var concurrentBag = compilation.GetTypeByMetadataNameOrThrow("System.Collections.Concurrent.ConcurrentBag`1");
        var concurrentDictionary2= compilation.GetTypeByMetadataNameOrThrow("System.Collections.Concurrent.ConcurrentDictionary`2");
        var runtimeTypeHandle = compilation.GetTypeByMetadataNameOrThrow("System.RuntimeTypeHandle");
        var @object = compilation.GetTypeByMetadataNameOrThrow("System.Object");
        var list = compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.List`1");
        var concurrentStack = compilation.GetTypeByMetadataNameOrThrow("System.Collections.Concurrent.ConcurrentStack`1");
        var valueTask1 = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
        var enumerable1 = compilation.GetTypeByMetadataNameOrThrow("System.Collections.Generic.IEnumerable`1");
        var exception = compilation.GetTypeByMetadataNameOrThrow("System.Exception");
        var task1 = compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.Task`1");
        var aggregateException = compilation.GetTypeByMetadataNameOrThrow("System.AggregateException");
        var taskCompletionSource1 = compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.TaskCompletionSource`1");
        
        return new WellKnownTypes(
            IDisposable: iDisposable,
            IAsyncDisposable: iAsyncDisposable,
            Lazy1: compilation.GetTypeByMetadataNameOrThrow("System.Lazy`1"),
            Func1: compilation.GetTypeByMetadataNameOrThrow("System.Func`1"),
            Func2: compilation.GetTypeByMetadataNameOrThrow("System.Func`2"),
            Func3: compilation.GetTypeByMetadataNameOrThrow("System.Func`3"),
            Func4: compilation.GetTypeByMetadataNameOrThrow("System.Func`4"),
            Func5: compilation.GetTypeByMetadataNameOrThrow("System.Func`5"),
            Func6: compilation.GetTypeByMetadataNameOrThrow("System.Func`6"),
            Func7: compilation.GetTypeByMetadataNameOrThrow("System.Func`7"),
            Func8: compilation.GetTypeByMetadataNameOrThrow("System.Func`8"),
            Func9: compilation.GetTypeByMetadataNameOrThrow("System.Func`9"),
            Func10: compilation.GetTypeByMetadataNameOrThrow("System.Func`10"),
            Func11: compilation.GetTypeByMetadataNameOrThrow("System.Func`11"),
            Func12: compilation.GetTypeByMetadataNameOrThrow("System.Func`12"),
            Func13: compilation.GetTypeByMetadataNameOrThrow("System.Func`13"),
            Func14: compilation.GetTypeByMetadataNameOrThrow("System.Func`14"),
            Func15: compilation.GetTypeByMetadataNameOrThrow("System.Func`15"),
            Func16: compilation.GetTypeByMetadataNameOrThrow("System.Func`16"),
            Func17: compilation.GetTypeByMetadataNameOrThrow("System.Func`17"),
            ThreadLocal1: compilation.GetTypeByMetadataNameOrThrow("System.Threading.ThreadLocal`1"),
            ValueTask: compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask"),
            ValueTask1: valueTask1,
            Task: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Tasks.Task"),
            Task1: task1,
            TaskCompletionSource1: taskCompletionSource1,
            TaskCompletionSourceOfInt: taskCompletionSource1.Construct(compilation.GetSpecialType(SpecialType.System_Int32)),
            SpinWait: compilation.GetTypeByMetadataNameOrThrow("System.Threading.SpinWait"),
            Thread: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Thread"),
            Action: compilation.GetTypeByMetadataNameOrThrow("System.Action"),
            ObjectDisposedException: compilation.GetTypeByMetadataNameOrThrow("System.ObjectDisposedException"),
            ConcurrentBagOfSyncDisposable: concurrentBag.Construct(iDisposable),
            ConcurrentBagOfAsyncDisposable: iAsyncDisposable is not null ? concurrentBag.Construct(iAsyncDisposable) : null,
            ConcurrentDictionaryOfSyncDisposable: concurrentDictionary2.Construct(iDisposable, iDisposable),
            ConcurrentDictionaryOfAsyncDisposable: iAsyncDisposable is not null ? concurrentDictionary2.Construct(iAsyncDisposable, iAsyncDisposable) : null,
            ConcurrentDictionaryOfRuntimeTypeHandleToObject: concurrentDictionary2.Construct(runtimeTypeHandle, @object),
            ListOfObject: list.Construct(@object),
            ListOfListOfObject: list.Construct(list.Construct(@object)),
            ConcurrentStackOfObject: concurrentStack.Construct(@object),
            ConcurrentStackOfConcurrentStackOfObject: concurrentStack.Construct(concurrentStack.Construct(@object)),
            Exception: exception,
            IEnumerableOfException: enumerable1.Construct(exception),
            IAsyncEnumerableOfException: compilation.GetTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1")?.Construct(exception),
            ListOfException: list.Construct(exception),
            AggregateException: aggregateException,
            TaskOfException: task1.Construct(exception),
            TaskOfNullableAggregateException: task1.Construct(aggregateException.WithNullableAnnotation(NullableAnnotation.Annotated)),
            SemaphoreSlim: compilation.GetTypeByMetadataNameOrThrow("System.Threading.SemaphoreSlim"),
            Int32: compilation.GetTypeByMetadataNameOrThrow("System.Int32"),
            Nullable1: compilation.GetTypeByMetadataNameOrThrow("System.Nullable`1"),
            Interlocked: compilation.GetTypeByMetadataNameOrThrow("System.Threading.Interlocked"),
            Type: compilation.GetTypeByMetadataNameOrThrow("System.Type"),
            MethodInfo: compilation.GetTypeByMetadataNameOrThrow("System.Reflection.MethodInfo"),
            String: compilation.GetTypeByMetadataNameOrThrow("System.String"),
            Object: @object,
            Boolean: compilation.GetTypeByMetadataNameOrThrow("System.Boolean"));
    }
}