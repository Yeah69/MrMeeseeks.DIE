using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE;

internal record WellKnownTypes(
    INamedTypeSymbol Container,
    INamedTypeSymbol SpyAggregationAttribute,
    INamedTypeSymbol SpyConstructorChoiceAggregationAttribute,
    INamedTypeSymbol ImplementationAggregationAttribute,
    INamedTypeSymbol TransientAggregationAttribute,
    INamedTypeSymbol ContainerInstanceAggregationAttribute,
    INamedTypeSymbol TransientScopeInstanceAggregationAttribute,
    INamedTypeSymbol ScopeInstanceAggregationAttribute,
    INamedTypeSymbol TransientScopeRootAggregationAttribute,
    INamedTypeSymbol ScopeRootAggregationAttribute,
    INamedTypeSymbol DecoratorAggregationAttribute,
    INamedTypeSymbol CompositeAggregationAttribute,
    INamedTypeSymbol DecoratorSequenceChoiceAttribute,
    INamedTypeSymbol ConstructorChoiceAttribute,
    INamedTypeSymbol TypeInitializerAttribute,
    INamedTypeSymbol FilterSpyAggregationAttribute,
    INamedTypeSymbol FilterSpyConstructorChoiceAggregationAttribute,
    INamedTypeSymbol FilterImplementationAggregationAttribute,
    INamedTypeSymbol FilterTransientAggregationAttribute,
    INamedTypeSymbol FilterContainerInstanceAggregationAttribute,
    INamedTypeSymbol FilterTransientScopeInstanceAggregationAttribute,
    INamedTypeSymbol FilterScopeInstanceAggregationAttribute,
    INamedTypeSymbol FilterTransientScopeRootAggregationAttribute,
    INamedTypeSymbol FilterScopeRootAggregationAttribute,
    INamedTypeSymbol FilterDecoratorAggregationAttribute,
    INamedTypeSymbol FilterCompositeAggregationAttribute,
    INamedTypeSymbol FilterDecoratorSequenceChoiceAttribute,
    INamedTypeSymbol FilterConstructorChoiceAttribute,
    INamedTypeSymbol FilterTypeInitializerAttribute,
    INamedTypeSymbol CustomScopeForRootTypesAttribute,
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
    INamedTypeSymbol ConcurrentBagOfDisposable,
    INamedTypeSymbol Action,
    INamedTypeSymbol Func,
    INamedTypeSymbol Exception,
    INamedTypeSymbol TaskCanceledException,
    INamedTypeSymbol SemaphoreSlim)
{
    internal static bool TryCreate(Compilation compilation, out WellKnownTypes wellKnownTypes)
    {
        var iContainer = compilation.GetTypeOrReport("MrMeeseeks.DIE.IContainer`1");
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
        var concurrentBagOfDisposable = iDisposable is null
            ? null
            : concurrentBag?.Construct(iDisposable);
        var action = compilation.GetTypeOrReport("System.Action");
        var func = compilation.GetTypeOrReport("System.Func`3");
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

        var decoratorSequenceChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(DecoratorSequenceChoiceAttribute).FullName ?? "");

        var constructorChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(ConstructorChoiceAttribute).FullName ?? "");

        var filterSpyAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterSpyAggregationAttribute).FullName ?? "");

        var filterSpyConstructorChoiceAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterSpyConstructorChoiceAggregationAttribute).FullName ?? "");

        var filterImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterImplementationAggregationAttribute).FullName ?? "");

        var filterTransientAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterTransientAggregationAttribute).FullName ?? "");

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

        var customScopeForRootTypesAttribute = compilation
            .GetTypeByMetadataName(typeof(CustomScopeForRootTypesAttribute).FullName ?? "");;

        var typeInitializerAttribute = compilation
            .GetTypeByMetadataName(typeof(TypeInitializerAttribute).FullName ?? "");;

        var filterTypeInitializerAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterTypeInitializerAttribute).FullName ?? "");

        if (iContainer is not null
            && spyAggregationAttribute is not null
            && spyConstructorChoiceAggregationAttribute is not null
            && implementationAggregationAttribute is not null
            && transientAggregationAttribute is not null
            && containerInstanceAggregationAttribute is not null
            && transientScopeInstanceAggregationAttribute is not null
            && scopeInstanceAggregationAttribute is not null
            && transientScopeRootAggregationAttribute is not null
            && scopeRootAggregationAttribute is not null
            && decoratorAggregationAttribute is not null
            && compositeAggregationAttribute is not null
            && decoratorSequenceChoiceAttribute is not null
            && constructorChoiceAttribute is not null
            && typeInitializerAttribute is not null
            && filterSpyAggregationAttribute is not null
            && filterSpyConstructorChoiceAggregationAttribute is not null
            && filterImplementationAggregationAttribute is not null
            && filterTransientAggregationAttribute is not null
            && filterContainerInstanceAggregationAttribute is not null
            && filterTransientScopeInstanceAggregationAttribute is not null
            && filterScopeInstanceAggregationAttribute is not null
            && filterTransientScopeRootAggregationAttribute is not null
            && filterScopeRootAggregationAttribute is not null
            && filterDecoratorAggregationAttribute is not null
            && filterCompositeAggregationAttribute is not null
            && filterDecoratorSequenceChoiceAttribute is not null
            && filterConstructorChoiceAttribute is not null
            && filterTypeInitializerAttribute is not null
            && customScopeForRootTypesAttribute is not null
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
            && concurrentBagOfDisposable is not null
            && action is not null
            && func is not null
            && exception is not null
            && semaphoreSlim is not null)
        {

            wellKnownTypes = new WellKnownTypes(
                Container: iContainer,
                SpyAggregationAttribute: spyAggregationAttribute,
                SpyConstructorChoiceAggregationAttribute: spyConstructorChoiceAggregationAttribute,
                ImplementationAggregationAttribute: implementationAggregationAttribute,
                TransientAggregationAttribute: transientAggregationAttribute,
                ContainerInstanceAggregationAttribute: containerInstanceAggregationAttribute,
                TransientScopeInstanceAggregationAttribute: transientScopeInstanceAggregationAttribute,
                ScopeInstanceAggregationAttribute: scopeInstanceAggregationAttribute,
                TransientScopeRootAggregationAttribute: transientScopeRootAggregationAttribute,
                ScopeRootAggregationAttribute: scopeRootAggregationAttribute,
                DecoratorAggregationAttribute: decoratorAggregationAttribute,
                CompositeAggregationAttribute: compositeAggregationAttribute,
                DecoratorSequenceChoiceAttribute: decoratorSequenceChoiceAttribute,
                ConstructorChoiceAttribute: constructorChoiceAttribute,
                TypeInitializerAttribute: typeInitializerAttribute,
                FilterSpyAggregationAttribute: filterSpyAggregationAttribute,
                FilterSpyConstructorChoiceAggregationAttribute: filterSpyConstructorChoiceAggregationAttribute,
                FilterImplementationAggregationAttribute: filterImplementationAggregationAttribute,
                FilterTransientAggregationAttribute: filterTransientAggregationAttribute,
                FilterContainerInstanceAggregationAttribute: filterContainerInstanceAggregationAttribute,
                FilterTransientScopeInstanceAggregationAttribute: filterTransientScopeInstanceAggregationAttribute,
                FilterScopeInstanceAggregationAttribute: filterScopeInstanceAggregationAttribute,
                FilterTransientScopeRootAggregationAttribute: filterTransientScopeRootAggregationAttribute,
                FilterScopeRootAggregationAttribute: filterScopeRootAggregationAttribute,
                FilterDecoratorAggregationAttribute: filterDecoratorAggregationAttribute,
                FilterCompositeAggregationAttribute: filterCompositeAggregationAttribute,
                FilterDecoratorSequenceChoiceAttribute: filterDecoratorSequenceChoiceAttribute,
                FilterConstructorChoiceAttribute: filterConstructorChoiceAttribute,
                FilterTypeInitializerAttribute: filterTypeInitializerAttribute,
                CustomScopeForRootTypesAttribute: customScopeForRootTypesAttribute,
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
                ConcurrentBagOfDisposable: concurrentBagOfDisposable,
                Action: action,
                Func: func,
                Exception: exception,
                TaskCanceledException: taskCanceledException,
                SemaphoreSlim: semaphoreSlim);
            return true;
        }
        
        wellKnownTypes = null!;
        return false;
    }
}