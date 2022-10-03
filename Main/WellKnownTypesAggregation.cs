using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE;

internal record WellKnownTypesAggregation(
    INamedTypeSymbol ImplementationAggregationAttribute,
    INamedTypeSymbol TransientAbstractionAggregationAttribute,
    INamedTypeSymbol SyncTransientAbstractionAggregationAttribute,
    INamedTypeSymbol AsyncTransientAbstractionAggregationAttribute,
    INamedTypeSymbol ContainerInstanceAbstractionAggregationAttribute,
    INamedTypeSymbol TransientScopeInstanceAbstractionAggregationAttribute,
    INamedTypeSymbol ScopeInstanceAbstractionAggregationAttribute,
    INamedTypeSymbol TransientScopeRootAbstractionAggregationAttribute,
    INamedTypeSymbol ScopeRootAbstractionAggregationAttribute,
    INamedTypeSymbol DecoratorAbstractionAggregationAttribute,
    INamedTypeSymbol CompositeAbstractionAggregationAttribute,
    INamedTypeSymbol TransientImplementationAggregationAttribute,
    INamedTypeSymbol SyncTransientImplementationAggregationAttribute,
    INamedTypeSymbol AsyncTransientImplementationAggregationAttribute,
    INamedTypeSymbol ContainerInstanceImplementationAggregationAttribute,
    INamedTypeSymbol TransientScopeInstanceImplementationAggregationAttribute,
    INamedTypeSymbol ScopeInstanceImplementationAggregationAttribute,
    INamedTypeSymbol TransientScopeRootImplementationAggregationAttribute,
    INamedTypeSymbol ScopeRootImplementationAggregationAttribute,
    INamedTypeSymbol AllImplementationsAggregationAttribute,
    INamedTypeSymbol AssemblyImplementationsAggregationAttribute,
    INamedTypeSymbol FilterImplementationAggregationAttribute,
    INamedTypeSymbol FilterTransientAbstractionAggregationAttribute,
    INamedTypeSymbol FilterSyncTransientAbstractionAggregationAttribute,
    INamedTypeSymbol FilterAsyncTransientAbstractionAggregationAttribute,
    INamedTypeSymbol FilterContainerInstanceAbstractionAggregationAttribute,
    INamedTypeSymbol FilterTransientScopeInstanceAbstractionAggregationAttribute,
    INamedTypeSymbol FilterScopeInstanceAbstractionAggregationAttribute,
    INamedTypeSymbol FilterTransientScopeRootAbstractionAggregationAttribute,
    INamedTypeSymbol FilterScopeRootAbstractionAggregationAttribute,
    INamedTypeSymbol FilterDecoratorAbstractionAggregationAttribute,
    INamedTypeSymbol FilterCompositeAbstractionAggregationAttribute,
    INamedTypeSymbol FilterTransientImplementationAggregationAttribute,
    INamedTypeSymbol FilterSyncTransientImplementationAggregationAttribute,
    INamedTypeSymbol FilterAsyncTransientImplementationAggregationAttribute,
    INamedTypeSymbol FilterContainerInstanceImplementationAggregationAttribute,
    INamedTypeSymbol FilterTransientScopeInstanceImplementationAggregationAttribute,
    INamedTypeSymbol FilterScopeInstanceImplementationAggregationAttribute,
    INamedTypeSymbol FilterTransientScopeRootImplementationAggregationAttribute,
    INamedTypeSymbol FilterScopeRootImplementationAggregationAttribute,
    INamedTypeSymbol FilterAllImplementationsAggregationAttribute,
    INamedTypeSymbol FilterAssemblyImplementationsAggregationAttribute)
{
    internal static bool TryCreate(Compilation compilation, out WellKnownTypesAggregation wellKnownTypes)
    {
        var implementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(ImplementationAggregationAttribute).FullName ?? "");

        var transientAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(TransientAbstractionAggregationAttribute).FullName ?? "");

        var syncTransientAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(SyncTransientAbstractionAggregationAttribute).FullName ?? "");

        var asyncTransientAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(AsyncTransientAbstractionAggregationAttribute).FullName ?? "");

        var containerInstanceAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(ContainerInstanceAbstractionAggregationAttribute).FullName ?? "");

        var transientScopeInstanceAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(TransientScopeInstanceAbstractionAggregationAttribute).FullName ?? "");

        var scopeInstanceAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(ScopeInstanceAbstractionAggregationAttribute).FullName ?? "");

        var transientScopeRootAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(TransientScopeRootAbstractionAggregationAttribute).FullName ?? "");

        var scopeRootAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(ScopeRootAbstractionAggregationAttribute).FullName ?? "");

        var decoratorAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(DecoratorAbstractionAggregationAttribute).FullName ?? "");

        var compositeAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(CompositeAbstractionAggregationAttribute).FullName ?? "");

        var transientImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(TransientImplementationAggregationAttribute).FullName ?? "");

        var syncTransientImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(SyncTransientImplementationAggregationAttribute).FullName ?? "");

        var asyncTransientImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(AsyncTransientImplementationAggregationAttribute).FullName ?? "");

        var containerInstanceImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(ContainerInstanceImplementationAggregationAttribute).FullName ?? "");

        var transientScopeInstanceImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(TransientScopeInstanceImplementationAggregationAttribute).FullName ?? "");

        var scopeInstanceImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(ScopeInstanceImplementationAggregationAttribute).FullName ?? "");

        var transientScopeRootImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(TransientScopeRootImplementationAggregationAttribute).FullName ?? "");

        var scopeRootImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(ScopeRootImplementationAggregationAttribute).FullName ?? "");

        var allImplementationsAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(AllImplementationsAggregationAttribute).FullName ?? "");

        var assemblyImplementationsAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(AssemblyImplementationsAggregationAttribute).FullName ?? "");

        var implementationChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(ImplementationChoiceAttribute).FullName ?? "");

        var implementationCollectionChoiceAttribute = compilation
            .GetTypeByMetadataName(typeof(ImplementationCollectionChoiceAttribute).FullName ?? "");

        var filterImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterImplementationAggregationAttribute).FullName ?? "");

        var filterTransientAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterTransientAbstractionAggregationAttribute).FullName ?? "");

        var filterSyncTransientAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterSyncTransientAbstractionAggregationAttribute).FullName ?? "");

        var filterAsyncTransientAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterAsyncTransientAbstractionAggregationAttribute).FullName ?? "");

        var filterContainerInstanceAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterContainerInstanceAbstractionAggregationAttribute).FullName ?? "");

        var filterTransientScopeInstanceAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterTransientScopeInstanceAbstractionAggregationAttribute).FullName ?? "");

        var filterScopeInstanceAggregationAbstractionAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterScopeInstanceAbstractionAggregationAttribute).FullName ?? "");

        var filterTransientScopeRootAggregationAbstractionAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterTransientScopeRootAbstractionAggregationAttribute).FullName ?? "");

        var filterScopeRootAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterScopeRootAbstractionAggregationAttribute).FullName ?? "");

        var filterDecoratorAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterDecoratorAbstractionAggregationAttribute).FullName ?? "");

        var filterCompositeAbstractionAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterCompositeAbstractionAggregationAttribute).FullName ?? "");

        var filterTransientImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterTransientImplementationAggregationAttribute).FullName ?? "");

        var filterSyncTransientImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterSyncTransientImplementationAggregationAttribute).FullName ?? "");

        var filterAsyncTransientImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterAsyncTransientImplementationAggregationAttribute).FullName ?? "");

        var filterContainerInstanceImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterContainerInstanceImplementationAggregationAttribute).FullName ?? "");

        var filterTransientScopeInstanceImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterTransientScopeInstanceImplementationAggregationAttribute).FullName ?? "");

        var filterScopeInstanceAggregationImplementationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterScopeInstanceImplementationAggregationAttribute).FullName ?? "");

        var filterTransientScopeRootAggregationImplementationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterTransientScopeRootImplementationAggregationAttribute).FullName ?? "");

        var filterScopeRootImplementationAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterScopeRootImplementationAggregationAttribute).FullName ?? "");

        var filterAllImplementationsAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterAllImplementationsAggregationAttribute).FullName ?? "");

        var filterAssemblyImplementationsAggregationAttribute = compilation
            .GetTypeByMetadataName(typeof(FilterAssemblyImplementationsAggregationAttribute).FullName ?? "");

        if (implementationAggregationAttribute is not null
            && transientAbstractionAggregationAttribute is not null
            && syncTransientAbstractionAggregationAttribute is not null
            && asyncTransientAbstractionAggregationAttribute is not null
            && containerInstanceAbstractionAggregationAttribute is not null
            && transientScopeInstanceAbstractionAggregationAttribute is not null
            && scopeInstanceAbstractionAggregationAttribute is not null
            && transientScopeRootAbstractionAggregationAttribute is not null
            && scopeRootAbstractionAggregationAttribute is not null
            && decoratorAbstractionAggregationAttribute is not null
            && compositeAbstractionAggregationAttribute is not null
            && transientImplementationAggregationAttribute is not null
            && syncTransientImplementationAggregationAttribute is not null
            && asyncTransientImplementationAggregationAttribute is not null
            && containerInstanceImplementationAggregationAttribute is not null
            && transientScopeInstanceImplementationAggregationAttribute is not null
            && scopeInstanceImplementationAggregationAttribute is not null
            && transientScopeRootImplementationAggregationAttribute is not null
            && scopeRootImplementationAggregationAttribute is not null
            && allImplementationsAggregationAttribute is not null
            && assemblyImplementationsAggregationAttribute is not null
            && implementationChoiceAttribute is not null
            && implementationCollectionChoiceAttribute is not null
            && filterImplementationAggregationAttribute is not null
            && filterTransientAbstractionAggregationAttribute is not null
            && filterSyncTransientAbstractionAggregationAttribute is not null
            && filterAsyncTransientAbstractionAggregationAttribute is not null
            && filterContainerInstanceAbstractionAggregationAttribute is not null
            && filterTransientScopeInstanceAbstractionAggregationAttribute is not null
            && filterScopeInstanceAggregationAbstractionAttribute is not null
            && filterTransientScopeRootAggregationAbstractionAttribute is not null
            && filterScopeRootAbstractionAggregationAttribute is not null
            && filterDecoratorAbstractionAggregationAttribute is not null
            && filterCompositeAbstractionAggregationAttribute is not null
            && filterTransientImplementationAggregationAttribute is not null
            && filterSyncTransientImplementationAggregationAttribute is not null
            && filterAsyncTransientImplementationAggregationAttribute is not null
            && filterContainerInstanceImplementationAggregationAttribute is not null
            && filterTransientScopeInstanceImplementationAggregationAttribute is not null
            && filterScopeInstanceAggregationImplementationAttribute is not null
            && filterTransientScopeRootAggregationImplementationAttribute is not null
            && filterScopeRootImplementationAggregationAttribute is not null
            && filterAllImplementationsAggregationAttribute is not null
            && filterAssemblyImplementationsAggregationAttribute is not null)
        {

            wellKnownTypes = new WellKnownTypesAggregation(
                ImplementationAggregationAttribute: implementationAggregationAttribute,
                TransientAbstractionAggregationAttribute: transientAbstractionAggregationAttribute,
                SyncTransientAbstractionAggregationAttribute: syncTransientAbstractionAggregationAttribute,
                AsyncTransientAbstractionAggregationAttribute: asyncTransientAbstractionAggregationAttribute,
                ContainerInstanceAbstractionAggregationAttribute: containerInstanceAbstractionAggregationAttribute,
                TransientScopeInstanceAbstractionAggregationAttribute: transientScopeInstanceAbstractionAggregationAttribute,
                ScopeInstanceAbstractionAggregationAttribute: scopeInstanceAbstractionAggregationAttribute,
                TransientScopeRootAbstractionAggregationAttribute: transientScopeRootAbstractionAggregationAttribute,
                ScopeRootAbstractionAggregationAttribute: scopeRootAbstractionAggregationAttribute,
                DecoratorAbstractionAggregationAttribute: decoratorAbstractionAggregationAttribute,
                CompositeAbstractionAggregationAttribute: compositeAbstractionAggregationAttribute,
                TransientImplementationAggregationAttribute: transientImplementationAggregationAttribute,
                SyncTransientImplementationAggregationAttribute: syncTransientImplementationAggregationAttribute,
                AsyncTransientImplementationAggregationAttribute: asyncTransientImplementationAggregationAttribute,
                ContainerInstanceImplementationAggregationAttribute: containerInstanceImplementationAggregationAttribute,
                TransientScopeInstanceImplementationAggregationAttribute: transientScopeInstanceImplementationAggregationAttribute,
                ScopeInstanceImplementationAggregationAttribute: scopeInstanceImplementationAggregationAttribute,
                TransientScopeRootImplementationAggregationAttribute: transientScopeRootImplementationAggregationAttribute,
                ScopeRootImplementationAggregationAttribute: scopeRootImplementationAggregationAttribute,
                AllImplementationsAggregationAttribute: allImplementationsAggregationAttribute,
                AssemblyImplementationsAggregationAttribute: assemblyImplementationsAggregationAttribute,
                FilterImplementationAggregationAttribute: filterImplementationAggregationAttribute,
                FilterTransientAbstractionAggregationAttribute: filterTransientAbstractionAggregationAttribute,
                FilterSyncTransientAbstractionAggregationAttribute: filterSyncTransientAbstractionAggregationAttribute,
                FilterAsyncTransientAbstractionAggregationAttribute: filterAsyncTransientAbstractionAggregationAttribute,
                FilterContainerInstanceAbstractionAggregationAttribute: filterContainerInstanceAbstractionAggregationAttribute,
                FilterTransientScopeInstanceAbstractionAggregationAttribute: filterTransientScopeInstanceAbstractionAggregationAttribute,
                FilterScopeInstanceAbstractionAggregationAttribute: filterScopeInstanceAggregationAbstractionAttribute,
                FilterTransientScopeRootAbstractionAggregationAttribute: filterTransientScopeRootAggregationAbstractionAttribute,
                FilterScopeRootAbstractionAggregationAttribute: filterScopeRootAbstractionAggregationAttribute,
                FilterDecoratorAbstractionAggregationAttribute: filterDecoratorAbstractionAggregationAttribute,
                FilterCompositeAbstractionAggregationAttribute: filterCompositeAbstractionAggregationAttribute,
                FilterTransientImplementationAggregationAttribute: filterTransientImplementationAggregationAttribute,
                FilterSyncTransientImplementationAggregationAttribute: filterSyncTransientImplementationAggregationAttribute,
                FilterAsyncTransientImplementationAggregationAttribute: filterAsyncTransientImplementationAggregationAttribute,
                FilterContainerInstanceImplementationAggregationAttribute: filterContainerInstanceImplementationAggregationAttribute,
                FilterTransientScopeInstanceImplementationAggregationAttribute: filterTransientScopeInstanceImplementationAggregationAttribute,
                FilterScopeInstanceImplementationAggregationAttribute: filterScopeInstanceAggregationImplementationAttribute,
                FilterTransientScopeRootImplementationAggregationAttribute: filterTransientScopeRootAggregationImplementationAttribute,
                FilterScopeRootImplementationAggregationAttribute: filterScopeRootImplementationAggregationAttribute,
                FilterAllImplementationsAggregationAttribute: filterAllImplementationsAggregationAttribute,
                FilterAssemblyImplementationsAggregationAttribute: filterAssemblyImplementationsAggregationAttribute);
            return true;
        }
        
        wellKnownTypes = null!;
        return false;
    }
}