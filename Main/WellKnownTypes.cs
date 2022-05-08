using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE;

internal record WellKnownTypes(
    INamedTypeSymbol SpyAggregationAttribute,
    INamedTypeSymbol SpyConstructorChoiceAggregationAttribute,
    INamedTypeSymbol ImplementationAggregationAttribute,
    INamedTypeSymbol TransientAggregationAttribute,
    INamedTypeSymbol SyncTransientAggregationAttribute,
    INamedTypeSymbol AsyncTransientAggregationAttribute,
    INamedTypeSymbol ContainerInstanceAggregationAttribute,
    INamedTypeSymbol TransientScopeInstanceAggregationAttribute,
    INamedTypeSymbol ScopeInstanceAggregationAttribute,
    INamedTypeSymbol TransientScopeRootAggregationAttribute,
    INamedTypeSymbol ScopeRootAggregationAttribute,
    INamedTypeSymbol DecoratorAggregationAttribute,
    INamedTypeSymbol CompositeAggregationAttribute,
    INamedTypeSymbol GenericParameterSubstituteAggregationAttribute,
    INamedTypeSymbol GenericParameterChoiceAttribute,
    INamedTypeSymbol DecoratorSequenceChoiceAttribute,
    INamedTypeSymbol ConstructorChoiceAttribute,
    INamedTypeSymbol PropertyChoiceAttribute,
    INamedTypeSymbol TypeInitializerAttribute,
    INamedTypeSymbol FilterSpyAggregationAttribute,
    INamedTypeSymbol FilterSpyConstructorChoiceAggregationAttribute,
    INamedTypeSymbol FilterImplementationAggregationAttribute,
    INamedTypeSymbol FilterTransientAggregationAttribute,
    INamedTypeSymbol FilterSyncTransientAggregationAttribute,
    INamedTypeSymbol FilterAsyncTransientAggregationAttribute,
    INamedTypeSymbol FilterContainerInstanceAggregationAttribute,
    INamedTypeSymbol FilterTransientScopeInstanceAggregationAttribute,
    INamedTypeSymbol FilterScopeInstanceAggregationAttribute,
    INamedTypeSymbol FilterTransientScopeRootAggregationAttribute,
    INamedTypeSymbol FilterScopeRootAggregationAttribute,
    INamedTypeSymbol FilterDecoratorAggregationAttribute,
    INamedTypeSymbol FilterCompositeAggregationAttribute,
    INamedTypeSymbol FilterGenericParameterSubstituteAggregationAttribute,
    INamedTypeSymbol FilterGenericParameterChoiceAttribute,
    INamedTypeSymbol FilterDecoratorSequenceChoiceAttribute,
    INamedTypeSymbol FilterConstructorChoiceAttribute,
    INamedTypeSymbol FilterPropertyChoiceAttribute,
    INamedTypeSymbol FilterTypeInitializerAttribute,
    INamedTypeSymbol CustomScopeForRootTypesAttribute,
    INamedTypeSymbol CreateFunctionAttribute,
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
    INamedTypeSymbol SemaphoreSlim)
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

        var spyAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(SpyAggregationAttribute).FullName ?? "");

        var spyConstructorChoiceAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(SpyConstructorChoiceAggregationAttribute).FullName ?? "");

        var implementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(ImplementationAggregationAttribute).FullName ?? "");

        var transientAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(TransientAggregationAttribute).FullName ?? "");

        var syncTransientAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(SyncTransientAggregationAttribute).FullName ?? "");

        var asyncTransientAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(AsyncTransientAggregationAttribute).FullName ?? "");

        var containerInstanceAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(ContainerInstanceAggregationAttribute).FullName ?? "");

        var transientScopeInstanceAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(TransientScopeInstanceAggregationAttribute).FullName ?? "");

        var scopeInstanceAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(ScopeInstanceAggregationAttribute).FullName ?? "");

        var transientScopeRootAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(TransientScopeRootAggregationAttribute).FullName ?? "");

        var scopeRootAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(ScopeRootAggregationAttribute).FullName ?? "");

        var decoratorAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(DecoratorAggregationAttribute).FullName ?? "");

        var compositeAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(CompositeAggregationAttribute).FullName ?? "");

        var genericParameterSubstituteAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(GenericParameterSubstituteAggregationAttribute).FullName ?? "");

        var filterGenericParameterSubstituteAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterGenericParameterSubstituteAggregationAttribute).FullName ?? "");

        var genericParameterChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(GenericParameterChoiceAttribute).FullName ?? "");

        var filterGenericParameterChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterGenericParameterChoiceAttribute).FullName ?? "");

        var decoratorSequenceChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(DecoratorSequenceChoiceAttribute).FullName ?? "");

        var constructorChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(ConstructorChoiceAttribute).FullName ?? "");

        var propertyChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(PropertyChoiceAttribute).FullName ?? "");

        var filterSpyAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterSpyAggregationAttribute).FullName ?? "");

        var filterSpyConstructorChoiceAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterSpyConstructorChoiceAggregationAttribute).FullName ?? "");

        var filterImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterImplementationAggregationAttribute).FullName ?? "");

        var filterTransientAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterTransientAggregationAttribute).FullName ?? "");

        var filterSyncTransientAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterSyncTransientAggregationAttribute).FullName ?? "");

        var filterAsyncTransientAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterAsyncTransientAggregationAttribute).FullName ?? "");

        var filterContainerInstanceAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterContainerInstanceAggregationAttribute).FullName ?? "");

        var filterTransientScopeInstanceAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterTransientScopeInstanceAggregationAttribute).FullName ?? "");

        var filterScopeInstanceAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterScopeInstanceAggregationAttribute).FullName ?? "");

        var filterTransientScopeRootAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterTransientScopeRootAggregationAttribute).FullName ?? "");

        var filterScopeRootAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterScopeRootAggregationAttribute).FullName ?? "");

        var filterDecoratorAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterDecoratorAggregationAttribute).FullName ?? "");

        var filterCompositeAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterCompositeAggregationAttribute).FullName ?? "");

        var filterDecoratorSequenceChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterDecoratorSequenceChoiceAttribute).FullName ?? "");

        var filterConstructorChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterConstructorChoiceAttribute).FullName ?? "");

        var filterPropertyChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterPropertyChoiceAttribute).FullName ?? "");

        var customScopeForRootTypesAttribute = compilation
            .GetTypeByMetadataName(typeof(CustomScopeForRootTypesAttribute).FullName ?? "");

        var typeInitializerAttribute = compilation
            .GetTypeByMetadataName(typeof(TypeInitializerAttribute).FullName ?? "");

        var filterTypeInitializerAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterTypeInitializerAttribute).FullName ?? "");

        var createFunctionAttribute = compilation
            .GetTypeByMetadataName(typeof(CreateFunctionAttribute).FullName ?? "");

        if (spyAggregationAttribute is not null
            && spyConstructorChoiceAggregationAttribute is not null
            && implementationAggregationAttribute is not null
            && transientAggregationAttribute is not null
            && syncTransientAggregationAttribute is not null
            && asyncTransientAggregationAttribute is not null
            && containerInstanceAggregationAttribute is not null
            && transientScopeInstanceAggregationAttribute is not null
            && scopeInstanceAggregationAttribute is not null
            && transientScopeRootAggregationAttribute is not null
            && scopeRootAggregationAttribute is not null
            && decoratorAggregationAttribute is not null
            && compositeAggregationAttribute is not null
            && genericParameterSubstituteAggregationAttribute is not null
            && genericParameterChoiceAttribute is not null
            && decoratorSequenceChoiceAttribute is not null
            && constructorChoiceAttribute is not null
            && propertyChoiceAttribute is not null
            && typeInitializerAttribute is not null
            && filterSpyAggregationAttribute is not null
            && filterSpyConstructorChoiceAggregationAttribute is not null
            && filterImplementationAggregationAttribute is not null
            && filterTransientAggregationAttribute is not null
            && filterSyncTransientAggregationAttribute is not null
            && filterAsyncTransientAggregationAttribute is not null
            && filterContainerInstanceAggregationAttribute is not null
            && filterTransientScopeInstanceAggregationAttribute is not null
            && filterScopeInstanceAggregationAttribute is not null
            && filterTransientScopeRootAggregationAttribute is not null
            && filterScopeRootAggregationAttribute is not null
            && filterDecoratorAggregationAttribute is not null
            && filterCompositeAggregationAttribute is not null
            && filterGenericParameterSubstituteAggregationAttribute is not null
            && filterGenericParameterChoiceAttribute is not null
            && filterDecoratorSequenceChoiceAttribute is not null
            && filterConstructorChoiceAttribute is not null
            && filterPropertyChoiceAttribute is not null
            && filterTypeInitializerAttribute is not null
            && customScopeForRootTypesAttribute is not null
            && createFunctionAttribute is not null
            && iDisposable is not null
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
            && semaphoreSlim is not null)
        {

            wellKnownTypes = new WellKnownTypes(
                SpyAggregationAttribute: spyAggregationAttribute,
                SpyConstructorChoiceAggregationAttribute: spyConstructorChoiceAggregationAttribute,
                ImplementationAggregationAttribute: implementationAggregationAttribute,
                TransientAggregationAttribute: transientAggregationAttribute,
                SyncTransientAggregationAttribute: syncTransientAggregationAttribute,
                AsyncTransientAggregationAttribute: asyncTransientAggregationAttribute,
                ContainerInstanceAggregationAttribute: containerInstanceAggregationAttribute,
                TransientScopeInstanceAggregationAttribute: transientScopeInstanceAggregationAttribute,
                ScopeInstanceAggregationAttribute: scopeInstanceAggregationAttribute,
                TransientScopeRootAggregationAttribute: transientScopeRootAggregationAttribute,
                ScopeRootAggregationAttribute: scopeRootAggregationAttribute,
                DecoratorAggregationAttribute: decoratorAggregationAttribute,
                CompositeAggregationAttribute: compositeAggregationAttribute,
                GenericParameterSubstituteAggregationAttribute: genericParameterSubstituteAggregationAttribute,
                GenericParameterChoiceAttribute: genericParameterChoiceAttribute,
                DecoratorSequenceChoiceAttribute: decoratorSequenceChoiceAttribute,
                ConstructorChoiceAttribute: constructorChoiceAttribute,
                PropertyChoiceAttribute: propertyChoiceAttribute,
                TypeInitializerAttribute: typeInitializerAttribute,
                FilterSpyAggregationAttribute: filterSpyAggregationAttribute,
                FilterSpyConstructorChoiceAggregationAttribute: filterSpyConstructorChoiceAggregationAttribute,
                FilterImplementationAggregationAttribute: filterImplementationAggregationAttribute,
                FilterTransientAggregationAttribute: filterTransientAggregationAttribute,
                FilterSyncTransientAggregationAttribute: filterSyncTransientAggregationAttribute,
                FilterAsyncTransientAggregationAttribute: filterAsyncTransientAggregationAttribute,
                FilterContainerInstanceAggregationAttribute: filterContainerInstanceAggregationAttribute,
                FilterTransientScopeInstanceAggregationAttribute: filterTransientScopeInstanceAggregationAttribute,
                FilterScopeInstanceAggregationAttribute: filterScopeInstanceAggregationAttribute,
                FilterTransientScopeRootAggregationAttribute: filterTransientScopeRootAggregationAttribute,
                FilterScopeRootAggregationAttribute: filterScopeRootAggregationAttribute,
                FilterDecoratorAggregationAttribute: filterDecoratorAggregationAttribute,
                FilterCompositeAggregationAttribute: filterCompositeAggregationAttribute,
                FilterGenericParameterSubstituteAggregationAttribute: filterGenericParameterSubstituteAggregationAttribute,
                FilterGenericParameterChoiceAttribute: filterGenericParameterChoiceAttribute,
                FilterDecoratorSequenceChoiceAttribute: filterDecoratorSequenceChoiceAttribute,
                FilterConstructorChoiceAttribute: filterConstructorChoiceAttribute,
                FilterPropertyChoiceAttribute: filterPropertyChoiceAttribute,
                FilterTypeInitializerAttribute: filterTypeInitializerAttribute,
                CustomScopeForRootTypesAttribute: customScopeForRootTypesAttribute,
                CreateFunctionAttribute: createFunctionAttribute,
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
                SemaphoreSlim: semaphoreSlim);
            return true;
        }
        
        wellKnownTypes = null!;
        return false;
    }
}