using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Validation.Attributes;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Configuration;

internal interface ITypesFromAttributesBase
{
    IImmutableSet<INamedTypeSymbol> Implementation { get; }
    IImmutableSet<INamedTypeSymbol> TransientAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> SyncTransientAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> AsyncTransientAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> ContainerInstanceAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> TransientScopeInstanceAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> ScopeInstanceAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> TransientScopeRootAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> ScopeRootAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> DecoratorAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> CompositeAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> TransientImplementation { get; }
    IImmutableSet<INamedTypeSymbol> SyncTransientImplementation { get; }
    IImmutableSet<INamedTypeSymbol> AsyncTransientImplementation { get; }
    IImmutableSet<INamedTypeSymbol> ContainerInstanceImplementation { get; }
    IImmutableSet<INamedTypeSymbol> TransientScopeInstanceImplementation { get; }
    IImmutableSet<INamedTypeSymbol> ScopeInstanceImplementation { get; }
    IImmutableSet<INamedTypeSymbol> TransientScopeRootImplementation { get; }
    IImmutableSet<INamedTypeSymbol> ScopeRootImplementation { get; }
    IImmutableSet<(INamedTypeSymbol, INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> DecoratorSequenceChoices { get; }
    IImmutableSet<(INamedTypeSymbol, IMethodSymbol)> ConstructorChoices { get; }
    IImmutableSet<(INamedTypeSymbol, IMethodSymbol)> Initializers { get; }
    IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol, IReadOnlyList<INamedTypeSymbol>)> GenericParameterSubstitutesChoices { get; } 
    IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol, INamedTypeSymbol)> GenericParameterChoices { get; } 
    IImmutableSet<(INamedTypeSymbol, IReadOnlyList<string>)> PropertyChoices { get; }
    bool AllImplementations { get; }
    IImmutableSet<IAssemblySymbol> AssemblyImplementations { get; }
    IImmutableSet<(INamedTypeSymbol, INamedTypeSymbol)> ImplementationChoices { get; }
    IImmutableSet<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> ImplementationCollectionChoices { get; }
    IImmutableSet<INamedTypeSymbol> InjectionKeyAttributeTypes { get; }
    IImmutableSet<(ITypeSymbol KeyType, object KeyValue, INamedTypeSymbol ImplementationType)> InjectionKeyChoices { get; }
    IImmutableSet<INamedTypeSymbol> DecorationOrdinalAttributeTypes { get; }
    IImmutableSet<(INamedTypeSymbol, int)> DecorationOrdinalChoices { get; }
    IImmutableSet<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> InterceptorChoices { get; }
    IImmutableSet<INamedTypeSymbol> FilterImplementation { get; }
    IImmutableSet<INamedTypeSymbol> FilterTransientAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> FilterSyncTransientAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> FilterAsyncTransientAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> FilterContainerInstanceAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> FilterTransientScopeInstanceAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> FilterScopeInstanceAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> FilterTransientScopeRootAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> FilterScopeRootAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> FilterDecoratorAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> FilterCompositeAbstraction { get; }
    IImmutableSet<INamedTypeSymbol> FilterTransientImplementation { get; }
    IImmutableSet<INamedTypeSymbol> FilterSyncTransientImplementation { get; }
    IImmutableSet<INamedTypeSymbol> FilterAsyncTransientImplementation { get; }
    IImmutableSet<INamedTypeSymbol> FilterContainerInstanceImplementation { get; }
    IImmutableSet<INamedTypeSymbol> FilterTransientScopeInstanceImplementation { get; }
    IImmutableSet<INamedTypeSymbol> FilterScopeInstanceImplementation { get; }
    IImmutableSet<INamedTypeSymbol> FilterTransientScopeRootImplementation { get; }
    IImmutableSet<INamedTypeSymbol> FilterScopeRootImplementation { get; }
    IImmutableSet<(INamedTypeSymbol, INamedTypeSymbol)> FilterDecoratorSequenceChoices { get; }
    IImmutableSet<INamedTypeSymbol> FilterConstructorChoices { get; }
    IImmutableSet<INamedTypeSymbol> FilterInitializers { get; }
    IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol)> FilterGenericParameterSubstitutesChoices { get; } 
    IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol)> FilterGenericParameterChoices { get; }
    IImmutableSet<INamedTypeSymbol> FilterPropertyChoices { get; }
    bool FilterAllImplementations { get; }
    IImmutableSet<IAssemblySymbol> FilterAssemblyImplementations { get; }
    IImmutableSet<INamedTypeSymbol> FilterImplementationChoices { get; }
    IImmutableSet<INamedTypeSymbol> FilterImplementationCollectionChoices { get; }
    IImmutableSet<INamedTypeSymbol> FilterInjectionKeyAttributeTypes { get; }
    IImmutableSet<(ITypeSymbol KeyType, object KeyValue, INamedTypeSymbol ImplementationType)> FilterInjectionKeyChoices { get; }
    IImmutableSet<INamedTypeSymbol> FilterDecorationOrdinalAttributeTypes { get; }
    IImmutableSet<INamedTypeSymbol> FilterDecorationOrdinalChoices { get; }
    IImmutableSet<INamedTypeSymbol> FilterInterceptorChoices { get; }
}

internal interface IAssemblyTypesFromAttributes : ITypesFromAttributesBase;

internal sealed class AssemblyTypesFromAttributes : TypesFromAttributesBase, IAssemblyTypesFromAttributes, IContainerInstance
{
    internal AssemblyTypesFromAttributes(
        Compilation compilation,
        ILocalDiagLogger localDiagLogger,
        IValidateAttributes validateAttributes,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesAggregation wellKnownTypesAggregation,
        WellKnownTypesChoice wellKnownTypesChoice,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        WellKnownTypesMapping wellKnownTypesMapping) 
        : base(
            compilation.Assembly.GetAttributes(), 
            null,
            null,
            localDiagLogger,
            validateAttributes,
            wellKnownTypes,
            wellKnownTypesAggregation,
            wellKnownTypesChoice,
            wellKnownTypesMiscellaneous,
            wellKnownTypesMapping)
    {
        ContainerInstanceAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.ContainerInstanceAbstractionAggregationAttribute);
        TransientScopeInstanceAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.TransientScopeInstanceAbstractionAggregationAttribute);
        TransientScopeRootAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.TransientScopeRootAbstractionAggregationAttribute);
        ScopeRootAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.ScopeRootAbstractionAggregationAttribute);
        ContainerInstanceImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.ContainerInstanceImplementationAggregationAttribute);
        TransientScopeInstanceImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.TransientScopeInstanceImplementationAggregationAttribute);
        TransientScopeRootImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.TransientScopeRootImplementationAggregationAttribute);
        ScopeRootImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.ScopeRootImplementationAggregationAttribute);
        FilterContainerInstanceAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterContainerInstanceAbstractionAggregationAttribute);
        FilterTransientScopeInstanceAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterTransientScopeInstanceAbstractionAggregationAttribute);
        FilterTransientScopeRootAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterTransientScopeRootAbstractionAggregationAttribute);
        FilterScopeRootAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterScopeRootAbstractionAggregationAttribute);
        FilterContainerInstanceImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.FilterContainerInstanceImplementationAggregationAttribute);
        FilterTransientScopeInstanceImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.FilterTransientScopeInstanceImplementationAggregationAttribute);
        FilterTransientScopeRootImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.FilterTransientScopeRootImplementationAggregationAttribute);
        FilterScopeRootImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.FilterScopeRootImplementationAggregationAttribute);
    }
}

internal interface IContainerTypesFromAttributes : ITypesFromAttributesBase;

internal sealed class ContainerTypesFromAttributes : TypesFromAttributesBase, IContainerTypesFromAttributes, IContainerInstance
{
    internal ContainerTypesFromAttributes(
        ILocalDiagLogger localDiagLogger,
        IValidateAttributes validateAttributes,
        IContainerInfo containerInfo,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesAggregation wellKnownTypesAggregation,
        WellKnownTypesChoice wellKnownTypesChoice,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        WellKnownTypesMapping wellKnownTypesMapping,
        IRangeUtility rangeUtility) 
        : base(
            rangeUtility.GetRangeAttributes(containerInfo.ContainerType), 
            containerInfo.ContainerType,
            containerInfo.ContainerType, 
            localDiagLogger,
            validateAttributes,
            wellKnownTypes,
            wellKnownTypesAggregation,
            wellKnownTypesChoice,
            wellKnownTypesMiscellaneous,
            wellKnownTypesMapping)
    {
        ContainerInstanceAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.ContainerInstanceAbstractionAggregationAttribute);
        TransientScopeInstanceAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.TransientScopeInstanceAbstractionAggregationAttribute);
        TransientScopeRootAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.TransientScopeRootAbstractionAggregationAttribute);
        ScopeRootAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.ScopeRootAbstractionAggregationAttribute);
        ContainerInstanceImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.ContainerInstanceImplementationAggregationAttribute);
        TransientScopeInstanceImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.TransientScopeInstanceImplementationAggregationAttribute);
        TransientScopeRootImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.TransientScopeRootImplementationAggregationAttribute);
        ScopeRootImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.ScopeRootImplementationAggregationAttribute);
        FilterContainerInstanceAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterContainerInstanceAbstractionAggregationAttribute);
        FilterTransientScopeInstanceAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterTransientScopeInstanceAbstractionAggregationAttribute);
        FilterTransientScopeRootAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterTransientScopeRootAbstractionAggregationAttribute);
        FilterScopeRootAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterScopeRootAbstractionAggregationAttribute);
        FilterContainerInstanceImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.FilterContainerInstanceImplementationAggregationAttribute);
        FilterTransientScopeInstanceImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.FilterTransientScopeInstanceImplementationAggregationAttribute);
        FilterTransientScopeRootImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.FilterTransientScopeRootImplementationAggregationAttribute);
        FilterScopeRootImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.FilterScopeRootImplementationAggregationAttribute);
    }
}

internal interface IScopeTypesFromAttributes : ITypesFromAttributesBase;

internal sealed class ScopeTypesFromAttributes : TypesFromAttributesBase, IScopeTypesFromAttributes, ITransientScopeInstance
{
    internal ScopeTypesFromAttributes(
        // parameter
        IScopeInfo scopeInfo,

        // dependencies
        ILocalDiagLogger localDiagLogger,
        IValidateAttributes validateAttributes,
        IContainerInfo containerInfo,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesAggregation wellKnownTypesAggregation,
        WellKnownTypesChoice wellKnownTypesChoice,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        WellKnownTypesMapping wellKnownTypesMapping,
        IRangeUtility rangeUtility)
        : base(
            scopeInfo.ScopeType is not null 
                ? rangeUtility.GetRangeAttributes(scopeInfo.ScopeType) 
                : [],
            scopeInfo.ScopeType,
            containerInfo.ContainerType,
            localDiagLogger,
            validateAttributes,
            wellKnownTypes,
            wellKnownTypesAggregation,
            wellKnownTypesChoice,
            wellKnownTypesMiscellaneous,
            wellKnownTypesMapping)
    {
    }
}

internal abstract class TypesFromAttributesBase : ITypesFromAttributesBase
{
    private readonly INamedTypeSymbol? _rangeType;
    private readonly INamedTypeSymbol? _containerType;
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly IValidateAttributes _validateAttributes;

    internal TypesFromAttributesBase(
        // parameter
        IReadOnlyList<AttributeData> attributeData,
        INamedTypeSymbol? rangeType,
        INamedTypeSymbol? containerType,

        // dependencies
        ILocalDiagLogger localDiagLogger,
        IValidateAttributes validateAttributes,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesAggregation wellKnownTypesAggregation,
        WellKnownTypesChoice wellKnownTypesChoice,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        WellKnownTypesMapping wellKnownTypesMapping)
    {
        _rangeType = rangeType;
        _containerType = containerType;
        _localDiagLogger = localDiagLogger;
        _validateAttributes = validateAttributes;
        AttributeDictionary = attributeData
            .GroupBy(ad => ad.AttributeClass, CustomSymbolEqualityComparer.Default)
            .ToDictionary(g => g.Key, g => g, CustomSymbolEqualityComparer.Default);
        
        Implementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.ImplementationAggregationAttribute);
        TransientAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.TransientAbstractionAggregationAttribute);
        SyncTransientAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.SyncTransientAbstractionAggregationAttribute);
        AsyncTransientAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.AsyncTransientAbstractionAggregationAttribute);
        ContainerInstanceAbstraction = ImmutableHashSet<INamedTypeSymbol>.Empty;
        TransientScopeInstanceAbstraction = ImmutableHashSet<INamedTypeSymbol>.Empty;
        ScopeInstanceAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.ScopeInstanceAbstractionAggregationAttribute);
        TransientScopeRootAbstraction = ImmutableHashSet<INamedTypeSymbol>.Empty;
        ScopeRootAbstraction = ImmutableHashSet<INamedTypeSymbol>.Empty;
        DecoratorAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.DecoratorAbstractionAggregationAttribute);
        CompositeAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.CompositeAbstractionAggregationAttribute);
        TransientImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.TransientImplementationAggregationAttribute);
        SyncTransientImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.SyncTransientImplementationAggregationAttribute);
        AsyncTransientImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.AsyncTransientImplementationAggregationAttribute);
        ContainerInstanceImplementation = ImmutableHashSet<INamedTypeSymbol>.Empty;
        TransientScopeInstanceImplementation = ImmutableHashSet<INamedTypeSymbol>.Empty;
        ScopeInstanceImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.ScopeInstanceImplementationAggregationAttribute);
        TransientScopeRootImplementation = ImmutableHashSet<INamedTypeSymbol>.Empty;
        ScopeRootImplementation = ImmutableHashSet<INamedTypeSymbol>.Empty;
        InjectionKeyAttributeTypes = GetTypesFromAttribute(wellKnownTypesMiscellaneous.InjectionKeyMappingAttribute)
            .Select(t => t.Item2)
            .ToImmutableHashSet();
        DecorationOrdinalAttributeTypes = GetTypesFromAttribute(wellKnownTypesMapping.DecorationOrdinalMappingAttribute)
            .Select(t => t.Item2)
            .ToImmutableHashSet();

        FilterImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.FilterImplementationAggregationAttribute);
        FilterTransientAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterTransientAbstractionAggregationAttribute);
        FilterSyncTransientAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterSyncTransientAbstractionAggregationAttribute);
        FilterAsyncTransientAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterAsyncTransientAbstractionAggregationAttribute);
        FilterContainerInstanceAbstraction = ImmutableHashSet<INamedTypeSymbol>.Empty;
        FilterTransientScopeInstanceAbstraction = ImmutableHashSet<INamedTypeSymbol>.Empty;
        FilterScopeInstanceAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterScopeInstanceAbstractionAggregationAttribute);
        FilterTransientScopeRootAbstraction = ImmutableHashSet<INamedTypeSymbol>.Empty;
        FilterScopeRootAbstraction = ImmutableHashSet<INamedTypeSymbol>.Empty;
        FilterDecoratorAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterDecoratorAbstractionAggregationAttribute);
        FilterCompositeAbstraction = GetAbstractionTypesFromAttribute(wellKnownTypesAggregation.FilterCompositeAbstractionAggregationAttribute);
        FilterTransientImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.FilterTransientImplementationAggregationAttribute);
        FilterSyncTransientImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.FilterSyncTransientImplementationAggregationAttribute);
        FilterAsyncTransientImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.FilterAsyncTransientImplementationAggregationAttribute);
        FilterContainerInstanceImplementation = ImmutableHashSet<INamedTypeSymbol>.Empty;
        FilterTransientScopeInstanceImplementation = ImmutableHashSet<INamedTypeSymbol>.Empty;
        FilterScopeInstanceImplementation = GetImplementationTypesFromAttribute(wellKnownTypesAggregation.FilterScopeInstanceImplementationAggregationAttribute);
        FilterTransientScopeRootImplementation = ImmutableHashSet<INamedTypeSymbol>.Empty;
        FilterScopeRootImplementation = ImmutableHashSet<INamedTypeSymbol>.Empty;
        FilterInjectionKeyAttributeTypes = GetTypesFromAttribute(wellKnownTypesMapping.FilterInjectionKeyMappingAttribute)
            .Select(t => t.Item2)
            .ToImmutableHashSet();
        FilterDecorationOrdinalAttributeTypes = GetTypesFromAttribute(wellKnownTypesMapping.FilterDecorationOrdinalMappingAttribute)
            .Select(t => t.Item2)
            .ToImmutableHashSet();
        
        void NotParsableAttribute(AttributeData ad) =>
            localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                ad,
                _rangeType,
                _containerType,
                "Not parsable attribute."),
                ad.GetLocation());
        
        DecoratorSequenceChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.DecoratorSequenceChoiceAttribute, out var decoratorSequenceChoiceAttributes) 
                ? decoratorSequenceChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length != 3
                    || ad.ConstructorArguments[0].Value is not INamedTypeSymbol interfaceType
                    || ad.ConstructorArguments[1].Value is not INamedTypeSymbol decoratedType)
                {
                    NotParsableAttribute(ad);
                    return null;
                }
                
                interfaceType = interfaceType.UnboundIfGeneric();
                
                if (!decoratedType
                        .OriginalDefinition
                        .AllDerivedTypesAndSelf()
                        .Select(t => t.UnboundIfGeneric())
                        .Contains(interfaceType, CustomSymbolEqualityComparer.Default))
                {
                    localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                        ad,
                        _rangeType,
                        _containerType,
                        $"Decorated type \"{decoratedType.FullName()}\" has to implement decorator interface \"{interfaceType.FullName()}\"."),
                        ad.GetLocation());
                    return null;
                }
                
                var decorators = ad
                    .ConstructorArguments[2]
                    .Values
                    .Select(tc =>
                    {
                        if (tc.Value is not INamedTypeSymbol decoratorType)
                        {
                            NotParsableAttribute(ad);
                            return null;
                        }

                        if (!decoratorType
                                .OriginalDefinition
                                .AllDerivedTypesAndSelf()
                                .Select(t => t.UnboundIfGeneric())
                                .Contains(interfaceType, CustomSymbolEqualityComparer.Default))
                        {
                            localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                                ad,
                                _rangeType,
                                _containerType,
                                $"Decorator type \"{decoratorType.FullName()}\" has to implement decorator interface \"{interfaceType.FullName()}\"."),
                                ad.GetLocation());
                            return null;
                        }
                        
                        return decoratorType;
                    })
                    .OfType<INamedTypeSymbol>()
                    .ToList();
                
                return ((INamedTypeSymbol, INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)?) (interfaceType, decoratedType, decorators);
            })
            .OfType<(INamedTypeSymbol, INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)>());
        
        FilterDecoratorSequenceChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterDecoratorSequenceChoiceAttribute, out var filterDecoratorSequenceChoiceAttributes) 
                ? filterDecoratorSequenceChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length != 2
                    || ad.ConstructorArguments[0].Value is not INamedTypeSymbol interfaceType
                    || ad.ConstructorArguments[1].Value is not INamedTypeSymbol decoratedType)
                {
                    NotParsableAttribute(ad);
                    return ((INamedTypeSymbol, INamedTypeSymbol)?) null;
                }

                interfaceType = interfaceType.UnboundIfGeneric();
                
                if (!decoratedType
                        .OriginalDefinition
                        .AllDerivedTypesAndSelf()
                        .Select(t => t.UnboundIfGeneric())
                        .Contains(interfaceType, CustomSymbolEqualityComparer.Default))
                {
                    localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                        ad,
                        _rangeType,
                        _containerType,
                        $"Decorated type \"{decoratedType.FullName()}\" has to implement decorator interface \"{interfaceType.FullName()}\"."),
                        ad.GetLocation());
                    return null;
                }
                
                return (interfaceType, decoratedType);
            })
            .OfType<(INamedTypeSymbol, INamedTypeSymbol)>());

        ConstructorChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.ConstructorChoiceAttribute, out var constructorChoiceAttributes) 
                ? constructorChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 2)
                {
                    NotParsableAttribute(ad);
                    return ((INamedTypeSymbol, IMethodSymbol)?) null;
                }
                
                var implementationType = ad.ConstructorArguments[0].Value as INamedTypeSymbol;
                var parameterTypes = ad
                    .ConstructorArguments[1]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<INamedTypeSymbol>()
                    .ToList();

                if (implementationType is not null)
                {
                    implementationType = implementationType.OriginalDefinitionIfUnbound();

                    var constructorChoice = implementationType
                        .InstanceConstructors
                        .Where(c => c.Parameters.Length == parameterTypes.Count)
                        .SingleOrDefault(c => c.Parameters.Select(p => p.Type)
                            .Zip(parameterTypes,
                                (pLeft, pRight) => CustomSymbolEqualityComparer.Default.Equals(pLeft, pRight))
                            .All(b => b));

                    if (constructorChoice is not null)
                    {
                        return (implementationType, constructorChoice);
                    }

                    localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                        ad,
                        _rangeType,
                        _containerType,
                        $"Couldn't find constructor \"{implementationType.FullName()}({string.Join(", ", parameterTypes.Select(p => p.FullName()))})\"."),
                        ad.GetLocation());
                    
                    return null;
                }

                NotParsableAttribute(ad);
                return null;
            })
            .OfType<(INamedTypeSymbol, IMethodSymbol)>());

        INamedTypeSymbol? SingleTypeArgument(AttributeData ad)
        {
            if (ad.ConstructorArguments.Length == 1
                && ad.ConstructorArguments[0].Value is INamedTypeSymbol type) 
                return type;
            NotParsableAttribute(ad);
            return null;
        }

        FilterConstructorChoices = ImmutableHashSet.CreateRange<INamedTypeSymbol>(
            CustomSymbolEqualityComparer.Default,
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterConstructorChoiceAttribute, out var filterConstructorChoiceAttributes) 
                ? filterConstructorChoiceAttributes
                : Enumerable.Empty<AttributeData>())
            .Select(SingleTypeArgument)
            .OfType<INamedTypeSymbol>());

        PropertyChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.PropertyChoiceAttribute, out var propertyChoiceGroup) 
                ? propertyChoiceGroup 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 1)
                {
                    NotParsableAttribute(ad);
                    return ((INamedTypeSymbol, IReadOnlyList<string>)?) null;
                }

                if (ad.ConstructorArguments[0].Value is not INamedTypeSymbol implementationType)
                {
                    NotParsableAttribute(ad);
                    return null;
                }
                var parameterTypes = ad
                    .ConstructorArguments[1]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<string>()
                    .ToImmutableHashSet();

                var propertyNames = implementationType
                    .OriginalDefinitionIfUnbound()
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Select(ps => ps.Name)
                    .ToImmutableHashSet();
                
                foreach (var nonExistent in parameterTypes.Except(propertyNames))
                    localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                        ad,
                        _rangeType,
                        _containerType,
                        $"Couldn't find property \"{nonExistent}\" on \"{implementationType.FullName()}\"."),
                        ad.GetLocation());

                return (implementationType, propertyNames.Intersect(parameterTypes).ToList());
            })
            .OfType<(INamedTypeSymbol, IReadOnlyList<string>)>());

        FilterPropertyChoices = ImmutableHashSet.CreateRange<INamedTypeSymbol>(
            CustomSymbolEqualityComparer.Default,
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterPropertyChoiceAttribute, out var filterPropertyChoicesGroup)
                ? filterPropertyChoicesGroup
                : Enumerable.Empty<AttributeData>())
            .Select(SingleTypeArgument)
            .OfType<INamedTypeSymbol>());

        Initializers = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesMiscellaneous.InitializerAttribute, out var typeInitializerAttributes) 
                ? typeInitializerAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length != 2
                    || ad.ConstructorArguments[0].Value is not INamedTypeSymbol type
                    || ad.ConstructorArguments[1].Value is not string methodName)
                {
                    NotParsableAttribute(ad);
                    return ((INamedTypeSymbol, IMethodSymbol)?) null;
                }

                var initializationMethod = type
                    .OriginalDefinitionIfUnbound()
                    .GetMembers(methodName)
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault();

                if (initializationMethod is not null)
                {
                    if (!initializationMethod.ReturnsVoid
                        && (wellKnownTypes.ValueTask is null || !CustomSymbolEqualityComparer.Default.Equals(initializationMethod.ReturnType, wellKnownTypes.ValueTask))
                        && !CustomSymbolEqualityComparer.Default.Equals(initializationMethod.ReturnType, wellKnownTypes.Task))
                    {
                        localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                            ad,
                            _rangeType,
                            _containerType,
                            $"If method \"{methodName}\" on \"{type.FullName()}\" is to be used as initialize method, then it should return either nothing (void), {(wellKnownTypes.ValueTask is not null ? $"\"{wellKnownTypes.ValueTask.FullName()}\", " : "")}or \"{wellKnownTypes.Task.FullName()}\"."),
                            ad.GetLocation());
                        return null;
                    }
                    
                    return (type, initializationMethod);
                }
                
                localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                    ad,
                    _rangeType,
                    _containerType,
                    $"Couldn't find a method with the name \"{methodName}\" on \"{type.FullName()}\"."),
                    ad.GetLocation());
                return null;
            })
            .OfType<(INamedTypeSymbol, IMethodSymbol)>());
        
        FilterInitializers = ImmutableHashSet.CreateRange<INamedTypeSymbol>(
            CustomSymbolEqualityComparer.Default,
            (AttributeDictionary.TryGetValue(wellKnownTypesMiscellaneous.FilterInitializerAttribute, out var filterInitializerAttributes) 
                ? filterInitializerAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(SingleTypeArgument)
            .OfType<INamedTypeSymbol>());
        
        GenericParameterSubstitutesChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.GenericParameterSubstitutesChoiceAttribute, out var genericParameterSubstitutesChoiceAttributes) 
                ? genericParameterSubstitutesChoiceAttributes
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 3)
                {
                    NotParsableAttribute(ad);
                    return null;
                }
                var genericType = ad.ConstructorArguments[0].Value as INamedTypeSymbol;

                if (genericType is not { IsGenericType: true, IsUnboundGenericType: true }
                    || ad.ConstructorArguments[1].Value is not string nameOfGenericParameter)
                {
                    NotParsableAttribute(ad);
                    return null;
                }

                var typeParameterSymbol = genericType
                    .OriginalDefinition
                    .TypeArguments
                    .OfType<ITypeParameterSymbol>()
                    .FirstOrDefault(tps => tps.Name == nameOfGenericParameter);

                if (typeParameterSymbol is null)
                {
                    localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                        ad,
                        _rangeType,
                        _containerType,
                        $"Couldn't find the generic type parameter with the name \"{nameOfGenericParameter}\" on \"{genericType.FullName()}\"."),
                        ad.GetLocation());
                    return null;
                }

                var substitutes = ad
                    .ConstructorArguments[2]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<INamedTypeSymbol>()
                    .ToList();

                if (substitutes.Count != ad.ConstructorArguments[2].Values.Length)
                {
                    NotParsableAttribute(ad);
                    return null;
                }
                
                return ((INamedTypeSymbol, ITypeParameterSymbol, IReadOnlyList<INamedTypeSymbol>)?) (genericType, typeParameterSymbol, substitutes);
            })
            .OfType<(INamedTypeSymbol, ITypeParameterSymbol, IReadOnlyList<INamedTypeSymbol>)>());
        
        GenericParameterChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.GenericParameterChoiceAttribute, out var genericParameterChoiceAttributes) 
                ? genericParameterChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 3)
                {
                    NotParsableAttribute(ad);
                    return null;
                }
                var genericType = ad.ConstructorArguments[0].Value as INamedTypeSymbol;
                var typeChoice = ad.ConstructorArguments[2].Value as INamedTypeSymbol;

                if (genericType is not { IsGenericType: true, IsUnboundGenericType: true }
                    || ad.ConstructorArguments[1].Value is not string nameOfGenericParameter
                    || typeChoice is null)
                {
                    NotParsableAttribute(ad);
                    return null;
                }

                var typeParameterSymbol = genericType
                    .OriginalDefinition
                    .TypeArguments
                    .OfType<ITypeParameterSymbol>()
                    .FirstOrDefault(tps => tps.Name == nameOfGenericParameter);

                if (typeParameterSymbol is null)
                {
                    localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                        ad,
                        _rangeType,
                        _containerType,
                        $"Couldn't find the generic type parameter with the name \"{nameOfGenericParameter}\" on \"{genericType.FullName()}\"."),
                        ad.GetLocation());
                    return null;
                }
                
                return ((INamedTypeSymbol, ITypeParameterSymbol, INamedTypeSymbol)?) (genericType, typeParameterSymbol, typeChoice);
            })
            .OfType<(INamedTypeSymbol, ITypeParameterSymbol, INamedTypeSymbol)>());
        
        FilterGenericParameterSubstitutesChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterGenericParameterSubstitutesChoiceAttribute, out var filterGenericParameterSubstitutesChoiceAttributes) 
                ? filterGenericParameterSubstitutesChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 2)
                {
                    NotParsableAttribute(ad);
                    return null;
                }
                var genericType = ad.ConstructorArguments[0].Value as INamedTypeSymbol;

                if (genericType is not { IsGenericType: true, IsUnboundGenericType: true }
                    || ad.ConstructorArguments[1].Value is not string nameOfGenericParameter)
                {
                    NotParsableAttribute(ad);
                    return null;
                }

                var typeParameterSymbol = genericType
                    .OriginalDefinition
                    .TypeArguments
                    .OfType<ITypeParameterSymbol>()
                    .FirstOrDefault(tps => tps.Name == nameOfGenericParameter);

                if (typeParameterSymbol is null)
                {
                    localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                        ad,
                        _rangeType,
                        _containerType,
                        $"Couldn't find the generic type parameter with the name \"{nameOfGenericParameter}\" on \"{genericType.FullName()}\"."),
                        ad.GetLocation());
                    return null;
                }
                
                return ((INamedTypeSymbol, ITypeParameterSymbol)?) (genericType, typeParameterSymbol);
            })
            .OfType<(INamedTypeSymbol, ITypeParameterSymbol)>());
        
        FilterGenericParameterChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterGenericParameterChoiceAttribute, out var filterGenericParameterChoiceAttributes) 
                ? filterGenericParameterChoiceAttributes
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 2)
                {
                    NotParsableAttribute(ad);
                    return null;
                }
                var genericType = ad.ConstructorArguments[0].Value as INamedTypeSymbol;

                if (genericType is not { IsGenericType: true, IsUnboundGenericType: true }
                    || ad.ConstructorArguments[1].Value is not string nameOfGenericParameter)
                {
                    NotParsableAttribute(ad);
                    return null;
                }

                var typeParameterSymbol = genericType
                    .OriginalDefinition
                    .TypeArguments
                    .OfType<ITypeParameterSymbol>()
                    .FirstOrDefault(tps => tps.Name == nameOfGenericParameter);

                if (typeParameterSymbol is null)
                {
                    localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                        ad,
                        _rangeType,
                        _containerType,
                        $"Couldn't find the generic type parameter with the name \"{nameOfGenericParameter}\" on \"{genericType.FullName()}\"."),
                        ad.GetLocation());
                    return null;
                }
                
                return ((INamedTypeSymbol, ITypeParameterSymbol)?) (genericType, typeParameterSymbol);
            })
            .OfType<(INamedTypeSymbol, ITypeParameterSymbol)>());

        AllImplementations = (AttributeDictionary.TryGetValue(wellKnownTypesAggregation.AllImplementationsAggregationAttribute, out var allImplementationsAttributes)
                ? allImplementationsAttributes
                : Enumerable.Empty<AttributeData>())
            .Any();

        FilterAllImplementations = (AttributeDictionary.TryGetValue(wellKnownTypesAggregation.FilterAllImplementationsAggregationAttribute, out var filterAllImplementationsAttributes)
                ? filterAllImplementationsAttributes
                : Enumerable.Empty<AttributeData>())
            .Any();

        IImmutableSet<IAssemblySymbol> GetAssemblies(INamedTypeSymbol attributeType) =>
            ImmutableHashSet.CreateRange<IAssemblySymbol>(
                CustomSymbolEqualityComparer.Default,
                (AttributeDictionary.TryGetValue(attributeType, out var assemblyImplementationsAttributes)
                    ? assemblyImplementationsAttributes
                    : Enumerable.Empty<AttributeData>())
                .SelectMany(ad => ad.ConstructorArguments.Length < 1
                    ? []
                    : ad.ConstructorArguments[0]
                        .Values
                        .Select(tc =>
                        {
                            if (tc.Value is not INamedTypeSymbol)
                            {
                                NotParsableAttribute(ad);
                                return null;
                            }
                            return tc.Value;
                        })
                        .OfType<INamedTypeSymbol>()
                        .Select(t =>
                        {
                            if (t.ContainingAssembly is null)
                            {
                                localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                                    ad,
                                    _rangeType,
                                    _containerType,
                                    $"Type \"{t.FullName()}\" doesn't lead to a single known assembly."),
                                    ad.GetLocation());
                            }
                            return t.ContainingAssembly;
                        })
                        .OfType<IAssemblySymbol>())
                .Distinct(CustomSymbolEqualityComparer.Default)
                .OfType<IAssemblySymbol>());

        AssemblyImplementations = GetAssemblies(wellKnownTypesAggregation.AssemblyImplementationsAggregationAttribute);

        FilterAssemblyImplementations = GetAssemblies(wellKnownTypesAggregation.FilterAssemblyImplementationsAggregationAttribute);
        
        ImplementationChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.ImplementationChoiceAttribute, out var implementationChoiceAttributes) 
                ? implementationChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 2
                    || ad.ConstructorArguments[0].Value is not INamedTypeSymbol type
                    || ad.ConstructorArguments[1].Value is not INamedTypeSymbol implementation)
                {
                    NotParsableAttribute(ad);
                    return null;
                }
                
                if (!validateAttributes.ValidateImplementation(implementation))
                {
                    localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                        ad,
                        _rangeType,
                        _containerType,
                        $"Type \"{implementation.FullName()}\" has to be an implementation."),
                        ad.GetLocation());
                    return null;
                }

                var unboundType = type.UnboundIfGeneric();
                if (!implementation
                        .OriginalDefinitionIfUnbound()
                        .AllDerivedTypesAndSelf()
                        .Select(t => t.UnboundIfGeneric())
                        .Contains(unboundType, CustomSymbolEqualityComparer.Default))
                {
                    localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                        ad,
                        _rangeType,
                        _containerType,
                        $"Type \"{implementation.FullName()}\" has to implement \"{type.FullName()}\"."),
                        ad.GetLocation());
                    return null;
                }
                
                return ((INamedTypeSymbol, INamedTypeSymbol)?)(type, implementation);
            })
            .OfType<(INamedTypeSymbol, INamedTypeSymbol)>());
        
        ImplementationCollectionChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.ImplementationCollectionChoiceAttribute, out var implementationCollectionChoicesAttributes) 
                ? implementationCollectionChoicesAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 2
                    || ad.ConstructorArguments[0].Value is not INamedTypeSymbol type)
                {
                    NotParsableAttribute(ad);
                    return null;
                }

                var unboundType = type.UnboundIfGeneric();
                var implementations = ad
                    .ConstructorArguments[1]
                    .Values
                    .Select(tc =>
                    {
                        if (tc.Value is not INamedTypeSymbol implementation)
                        {
                            NotParsableAttribute(ad);
                            return null;
                        }
                        
                        if (!implementation
                                .OriginalDefinitionIfUnbound()
                                .AllDerivedTypesAndSelf()
                                .Select(t => t.UnboundIfGeneric())
                                .Contains(unboundType, CustomSymbolEqualityComparer.Default))
                        {
                            localDiagLogger.Error(ErrorLogData.ValidationConfigurationAttribute(
                                ad,
                                _rangeType,
                                _containerType,
                                $"Type \"{implementation.FullName()}\" has to implement \"{type.FullName()}\"."),
                                ad.GetLocation());
                            return null;
                        }
                        
                        return tc.Value;
                    })
                    .OfType<INamedTypeSymbol>()
                    .ToList();
                
                return ((INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)?) (type, implementations);
            })
            .OfType<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)>());
        
        FilterImplementationChoices = GetAbstractionTypesFromAttribute(wellKnownTypesChoice.FilterImplementationChoiceAttribute);
        
        FilterImplementationCollectionChoices = GetAbstractionTypesFromAttribute(wellKnownTypesChoice.FilterImplementationCollectionChoiceAttribute);
        
        InjectionKeyChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.InjectionKeyChoiceAttribute, out var injectionKeyChoiceAttributes) 
                ? injectionKeyChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ParseInjectionKeyChoice)
            .Where(t => t.HasValue)
            .Select(t => t ?? throw new ImpossibleDieException()));
        
        FilterInjectionKeyChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterInjectionKeyChoiceAttribute, out var filterInjectionKeyChoiceAttributes) 
                ? filterInjectionKeyChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ParseInjectionKeyChoice)
            .Where(t => t.HasValue)
            .Select(t => t ?? throw new ImpossibleDieException()));

        DecorationOrdinalChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.DecorationOrdinalChoiceAttribute, out var decorationOrdinalChoiceAttributes) 
                ? decorationOrdinalChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ParseDecorationOrdinalChoice)
            .Where(t => t.HasValue)
            .Select(t => t ?? throw new ImpossibleDieException()));

        FilterDecorationOrdinalChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterDecorationOrdinalChoiceAttribute, out var filterDecorationOrdinalChoiceAttributes) 
                ? filterDecorationOrdinalChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ParseFilterDecorationOrdinalChoice)
            .OfType<INamedTypeSymbol>());
        
        InterceptorChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.InterceptorChoiceAttribute, out var interceptorChoiceAttributes) 
                ? interceptorChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ParseInterceptorChoice)
            .Where(t => t.HasValue)
            .Select(t => t ?? throw new ImpossibleDieException()));

        FilterInterceptorChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterInterceptorChoiceAttribute, out var filterInterceptorChoiceAttributes) 
                ? filterInterceptorChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ParseFilterInterceptorChoice)
            .OfType<INamedTypeSymbol>());
            
        
        return;

        (ITypeSymbol KeyType, object KeyValue, INamedTypeSymbol ImplementationType)? ParseInjectionKeyChoice(
            AttributeData injectionKeyChoiceAttribute)
        {
            if (injectionKeyChoiceAttribute.ConstructorArguments.Length < 2
                || injectionKeyChoiceAttribute.ConstructorArguments[0].Value is not { } keyValue
                || injectionKeyChoiceAttribute.ConstructorArguments[0].Type is not { } keyType
                || injectionKeyChoiceAttribute.ConstructorArguments[1].Value is not INamedTypeSymbol implementationType
                || !validateAttributes.ValidateImplementation(implementationType))
            {
                NotParsableAttribute(injectionKeyChoiceAttribute);
                return null;
            }

            return (keyType, keyValue, implementationType);
        }

        (INamedTypeSymbol DecorationImplementationType, int Ordinal)? ParseDecorationOrdinalChoice(
            AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length < 2
                || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol decorationImplementationType
                || attribute.ConstructorArguments[1].Value is not int ordinal
                || !validateAttributes.ValidateImplementation(decorationImplementationType))
            {
                NotParsableAttribute(attribute);
                return null;
            }

            return (decorationImplementationType, ordinal);
        }

        INamedTypeSymbol? ParseFilterDecorationOrdinalChoice(
            AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length < 1
                || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol filterDecorationImplementationType
                || !validateAttributes.ValidateImplementation(filterDecorationImplementationType))
            {
                NotParsableAttribute(attribute);
                return null;
            }

            return filterDecorationImplementationType;
        }

        (INamedTypeSymbol InterceptorImplementationType, IReadOnlyList<INamedTypeSymbol>)? ParseInterceptorChoice(
            AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length < 2
                || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol interceptorImplementationType
                || attribute.ConstructorArguments[1].Values is { Length: 0 }
                || attribute.ConstructorArguments[1].Values.Any(tc => tc.Value is not INamedTypeSymbol)
                || !validateAttributes.ValidateImplementation(interceptorImplementationType))
            {
                NotParsableAttribute(attribute);
                return null;
            }

            var abstractions = attribute.ConstructorArguments[1].Values.Select(tc => tc.Value).OfType<INamedTypeSymbol>().ToList();
            return (interceptorImplementationType, abstractions);
        }

        INamedTypeSymbol? ParseFilterInterceptorChoice(
            AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length < 1
                || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol filterInterceptorImplementationType
                || !validateAttributes.ValidateImplementation(filterInterceptorImplementationType))
            {
                NotParsableAttribute(attribute);
                return null;
            }

            return filterInterceptorImplementationType;
        }
    }

    private Dictionary<ISymbol?, IGrouping<ISymbol?, AttributeData>> AttributeDictionary { get; }
    
    protected IImmutableSet<INamedTypeSymbol> GetAbstractionTypesFromAttribute(
        INamedTypeSymbol attribute)
    {
        return ImmutableHashSet.CreateRange<INamedTypeSymbol>(
            CustomSymbolEqualityComparer.Default,
            GetTypesFromAttribute(attribute)
                .Where(t =>
                {
                    var ret = _validateAttributes.ValidateAbstraction(t.Item2);

                    if (!ret)
                        _localDiagLogger.Warning(WarningLogData.ValidationConfigurationAttribute(
                            t.Item1,
                            _rangeType, 
                            _containerType, 
                            $"Given type \"{t.Item2.FullName()}\" isn't a valid abstraction type. It'll be ignored."),
                            t.Item1.GetLocation());
                
                    return ret;
                })
                .Select(t => t.Item2));
    }
    
    protected IImmutableSet<INamedTypeSymbol> GetImplementationTypesFromAttribute(
        INamedTypeSymbol attribute)
    {
        return ImmutableHashSet.CreateRange<INamedTypeSymbol>(
            CustomSymbolEqualityComparer.Default,
            GetTypesFromAttribute(attribute)
            .Where(t =>
            {
                var ret = _validateAttributes.ValidateImplementation(t.Item2);

                if (!ret)
                    _localDiagLogger.Warning(WarningLogData.ValidationConfigurationAttribute(
                        t.Item1,
                        _rangeType, 
                        _containerType, 
                        $"Given type \"{t.Item2.FullName()}\" isn't a valid implementation type. It'll be ignored."),
                        t.Item1.GetLocation());
                
                return ret;
            })
            .Select(t => t.Item2));
    }
    
    private IEnumerable<(AttributeData, INamedTypeSymbol)> GetTypesFromAttribute(
        INamedTypeSymbol attribute) =>
        (AttributeDictionary.TryGetValue(attribute, out var attributes) ? attributes : Enumerable.Empty<AttributeData>())
        .SelectMany(ad => ad.ConstructorArguments
            .Where(tc => tc.Kind == TypedConstantKind.Type)
            .Select(tc => (ad, tc))
            .Concat(ad.ConstructorArguments.SelectMany(ca => ca.Kind is TypedConstantKind.Array
                ? (IEnumerable<(AttributeData, TypedConstant)>)ca.Values.Select(tc => (ad, tc))
                : Array.Empty<(AttributeData, TypedConstant)>())))
        .Select(t => t.Item2.Value is INamedTypeSymbol type
            ? (t.Item1, type.OriginalDefinition)
            : ((AttributeData, INamedTypeSymbol)?)null)
        .OfType<(AttributeData, INamedTypeSymbol)>();

    public IImmutableSet<INamedTypeSymbol> Implementation { get; }
    public IImmutableSet<INamedTypeSymbol> TransientAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> SyncTransientAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> AsyncTransientAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> ContainerInstanceAbstraction { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> TransientScopeInstanceAbstraction { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> ScopeInstanceAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> TransientScopeRootAbstraction { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> ScopeRootAbstraction { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> DecoratorAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> CompositeAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> TransientImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> SyncTransientImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> AsyncTransientImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> ContainerInstanceImplementation { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> TransientScopeInstanceImplementation { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> ScopeInstanceImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> TransientScopeRootImplementation { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> ScopeRootImplementation { get; protected set; }
    public IImmutableSet<(INamedTypeSymbol, INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> DecoratorSequenceChoices { get; }
    public IImmutableSet<(INamedTypeSymbol, IMethodSymbol)> ConstructorChoices { get; }
    public IImmutableSet<(INamedTypeSymbol, IMethodSymbol)> Initializers { get; }
    public IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol, IReadOnlyList<INamedTypeSymbol>)> GenericParameterSubstitutesChoices { get; }
    public IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol, INamedTypeSymbol)> GenericParameterChoices { get; }
    public IImmutableSet<(INamedTypeSymbol, IReadOnlyList<string>)> PropertyChoices { get; }
    public bool AllImplementations { get; }
    public IImmutableSet<IAssemblySymbol> AssemblyImplementations { get; }
    public IImmutableSet<(INamedTypeSymbol, INamedTypeSymbol)> ImplementationChoices { get; }
    public IImmutableSet<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> ImplementationCollectionChoices { get; }
    public IImmutableSet<INamedTypeSymbol> InjectionKeyAttributeTypes { get; }
    public IImmutableSet<(ITypeSymbol KeyType, object KeyValue, INamedTypeSymbol ImplementationType)> InjectionKeyChoices { get; }
    public IImmutableSet<INamedTypeSymbol> DecorationOrdinalAttributeTypes { get; }
    public IImmutableSet<(INamedTypeSymbol, int)> DecorationOrdinalChoices { get; }
    public IImmutableSet<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> InterceptorChoices { get; }

    public IImmutableSet<INamedTypeSymbol> FilterImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> FilterTransientAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> FilterSyncTransientAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> FilterAsyncTransientAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> FilterContainerInstanceAbstraction { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> FilterTransientScopeInstanceAbstraction { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> FilterScopeInstanceAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> FilterTransientScopeRootAbstraction { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> FilterScopeRootAbstraction { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> FilterDecoratorAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> FilterCompositeAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> FilterTransientImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> FilterSyncTransientImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> FilterAsyncTransientImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> FilterContainerInstanceImplementation { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> FilterTransientScopeInstanceImplementation { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> FilterScopeInstanceImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> FilterTransientScopeRootImplementation { get; protected set; }
    public IImmutableSet<INamedTypeSymbol> FilterScopeRootImplementation { get; protected set; }
    public IImmutableSet<(INamedTypeSymbol, INamedTypeSymbol)> FilterDecoratorSequenceChoices { get; }
    public IImmutableSet<INamedTypeSymbol> FilterConstructorChoices { get; }
    public IImmutableSet<INamedTypeSymbol> FilterInitializers { get; }
    public IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol)> FilterGenericParameterSubstitutesChoices { get; }
    public IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol)> FilterGenericParameterChoices { get; }
    public IImmutableSet<INamedTypeSymbol> FilterPropertyChoices { get; }
    public bool FilterAllImplementations { get; }
    public IImmutableSet<IAssemblySymbol> FilterAssemblyImplementations { get; }
    public IImmutableSet<INamedTypeSymbol> FilterImplementationChoices { get; }
    public IImmutableSet<INamedTypeSymbol> FilterImplementationCollectionChoices { get; }
    public IImmutableSet<INamedTypeSymbol> FilterInjectionKeyAttributeTypes { get; }
    public IImmutableSet<(ITypeSymbol KeyType, object KeyValue, INamedTypeSymbol ImplementationType)> FilterInjectionKeyChoices { get; }
    public IImmutableSet<INamedTypeSymbol> FilterDecorationOrdinalAttributeTypes { get; }
    public IImmutableSet<INamedTypeSymbol> FilterDecorationOrdinalChoices { get; }
    public IImmutableSet<INamedTypeSymbol> FilterInterceptorChoices { get; }
}