using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Validation.Attributes;

namespace MrMeeseeks.DIE.Configuration;

internal interface ITypesFromAttributes
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
    IImmutableSet<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> DecoratorSequenceChoices { get; }
    IImmutableSet<(INamedTypeSymbol, IMethodSymbol)> ConstructorChoices { get; }
    IImmutableSet<(INamedTypeSymbol, IMethodSymbol)> TypeInitializers { get; }
    IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol, IReadOnlyList<INamedTypeSymbol>)> GenericParameterSubstitutesChoices { get; } 
    IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol, INamedTypeSymbol)> GenericParameterChoices { get; } 
    IImmutableSet<(INamedTypeSymbol, IReadOnlyList<IPropertySymbol>)> PropertyChoices { get; }
    bool AllImplementations { get; }
    IImmutableSet<IAssemblySymbol> AssemblyImplementations { get; }
    IImmutableSet<(INamedTypeSymbol, INamedTypeSymbol)> ImplementationChoices { get; }
    IImmutableSet<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> ImplementationCollectionChoices { get; }
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
    IImmutableSet<INamedTypeSymbol> FilterDecoratorSequenceChoices { get; }
    IImmutableSet<INamedTypeSymbol> FilterConstructorChoices { get; }
    IImmutableSet<INamedTypeSymbol> FilterTypeInitializers { get; }
    IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol)> FilterGenericParameterSubstitutesChoices { get; } 
    IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol)> FilterGenericParameterChoices { get; }
    IImmutableSet<INamedTypeSymbol> FilterPropertyChoices { get; }
    bool FilterAllImplementations { get; }
    IImmutableSet<IAssemblySymbol> FilterAssemblyImplementations { get; }
    IImmutableSet<INamedTypeSymbol> FilterImplementationChoices { get; }
    IImmutableSet<INamedTypeSymbol> FilterImplementationCollectionChoices { get; }
}

internal class TypesFromAttributes : ScopeTypesFromAttributes
{
    internal TypesFromAttributes(
        // parameter
        IReadOnlyList<AttributeData> attributeData,
        INamedTypeSymbol? rangeType,
        INamedTypeSymbol? containerType,

        // dependencies
        IValidateAttributes validateAttributes,
        WellKnownTypesAggregation wellKnownTypesAggregation,
        WellKnownTypesChoice wellKnownTypesChoice,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous) 
        : base(attributeData, rangeType, containerType, validateAttributes, wellKnownTypesAggregation, wellKnownTypesChoice, wellKnownTypesMiscellaneous)
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

internal class ScopeTypesFromAttributes : ITypesFromAttributes
{
    private readonly INamedTypeSymbol? _rangeType;
    private readonly INamedTypeSymbol? _containerType;
    private readonly IValidateAttributes _validateAttributes;
    protected readonly List<Diagnostic> _warnings = new(); 
    protected readonly List<Diagnostic> _errors = new();

    internal ScopeTypesFromAttributes(
        // parameter
        IReadOnlyList<AttributeData> attributeData,
        INamedTypeSymbol? rangeType,
        INamedTypeSymbol? containerType,

        // dependencies
        IValidateAttributes validateAttributes,
        WellKnownTypesAggregation wellKnownTypesAggregation,
        WellKnownTypesChoice wellKnownTypesChoice,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous)
    {
        _rangeType = rangeType;
        _containerType = containerType;
        _validateAttributes = validateAttributes;
        AttributeDictionary = attributeData
            .GroupBy(ad => ad.AttributeClass, SymbolEqualityComparer.Default)
            .ToDictionary(g => g.Key, g => g);

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
        
        DecoratorSequenceChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.DecoratorSequenceChoiceAttribute, out var decoratorSequenceChoiceAttributes) 
                ? decoratorSequenceChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 2)
                    return null;
                var decoratedType = ad.ConstructorArguments[0].Value as INamedTypeSymbol;
                var decorators = ad
                    .ConstructorArguments[1]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<INamedTypeSymbol>()
                    .ToList();
                
                return decoratedType is null 
                    ? null 
                    : ((INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)?) (decoratedType, decorators);
            })
            .OfType<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)>());
        
        FilterDecoratorSequenceChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterDecoratorSequenceChoiceAttribute, out var filterDecoratorSequenceChoiceAttributes) 
                ? filterDecoratorSequenceChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length != 1)
                    return null;
                
                return ad.ConstructorArguments[0].Value as INamedTypeSymbol;
            })
            .OfType<INamedTypeSymbol>());

        ConstructorChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.ConstructorChoiceAttribute, out var constructorChoiceAttributes) 
                ? constructorChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 2)
                    return ((INamedTypeSymbol, IList<INamedTypeSymbol>)?) null;
                var implementationType = ad.ConstructorArguments[0].Value as INamedTypeSymbol;
                var parameterTypes = ad
                    .ConstructorArguments[1]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<INamedTypeSymbol>()
                    .ToList();

                return implementationType is {}
                    ? (implementationType, parameterTypes) 
                    : null;
            })
            .Select(t =>
            {
                if (t is null) return null;

                var implementationType = t.Value.Item1.OriginalDefinitionIfUnbound();
                var parameterTypes = t.Value.Item2;

                if (implementationType is null) return null;
                
                var constructorChoice = implementationType
                    .InstanceConstructors
                    .Where(c => c.Parameters.Length == parameterTypes.Count)
                    .SingleOrDefault(c => c.Parameters.Select(p => p.Type)
                        .Zip(parameterTypes,
                            (pLeft, pRight) => pLeft.Equals(pRight, SymbolEqualityComparer.Default))
                        .All(b => b));
                return constructorChoice is { }
                    ? (implementationType, constructorChoice)
                    : ((INamedTypeSymbol, IMethodSymbol)?)null;

            })
            .OfType<(INamedTypeSymbol, IMethodSymbol)>());

        FilterConstructorChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterConstructorChoiceAttribute, out var filterConstructorChoiceAttributes) 
                ? filterConstructorChoiceAttributes
                : Enumerable.Empty<AttributeData>())
            .Select(ad => ad.ConstructorArguments.Length < 1 
                ? null 
                : ad.ConstructorArguments[0].Value as INamedTypeSymbol)
            .OfType<INamedTypeSymbol>());

        PropertyChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.PropertyChoiceAttribute, out var propertyChoiceGroup) 
                ? propertyChoiceGroup 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 1)
                    return ((INamedTypeSymbol, IReadOnlyList<IPropertySymbol>)?) null;
                if (ad.ConstructorArguments[0].Value is not INamedTypeSymbol implementationType) 
                    return null;
                var parameterTypes = ad
                    .ConstructorArguments[1]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<string>()
                    .ToList();

                var properties = implementationType
                    .OriginalDefinitionIfUnbound()
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(ps => parameterTypes.Contains(ps.Name))
                    .ToList();

                return (implementationType, properties);
            })
            .OfType<(INamedTypeSymbol, IReadOnlyList<IPropertySymbol>)>());

        FilterPropertyChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterPropertyChoiceAttribute, out var filterPropertyChoicesGroup)
                ? filterPropertyChoicesGroup
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 1)
                    return null;
                return ad.ConstructorArguments[0].Value as INamedTypeSymbol;
            })
            .OfType<INamedTypeSymbol>());

        TypeInitializers = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesMiscellaneous.TypeInitializerAttribute, out var typeInitializerAttributes) 
                ? typeInitializerAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length != 2 
                    || ad.ConstructorArguments[0].Value is not INamedTypeSymbol type
                    || ad.ConstructorArguments[1].Value is not string methodName)
                    return ((INamedTypeSymbol, IMethodSymbol)?) null;

                var initializationMethod = type
                    .OriginalDefinitionIfUnbound()
                    .GetMembers(methodName)
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault(m => m.Parameters.Length == 0);

                if (initializationMethod is { })
                {
                    return (type, initializationMethod);
                }

                return null;
            })
            .OfType<(INamedTypeSymbol, IMethodSymbol)>());
        
        FilterTypeInitializers = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesMiscellaneous.FilterTypeInitializerAttribute, out var filterTypeInitializerAttributes) 
                ? filterTypeInitializerAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length != 1)
                    return null;
                return ad.ConstructorArguments[0].Value as INamedTypeSymbol;
            })
            .OfType<INamedTypeSymbol>());
        
        GenericParameterSubstitutesChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.GenericParameterSubstitutesChoiceAttribute, out var genericParameterSubstitutesChoiceAttributes) 
                ? genericParameterSubstitutesChoiceAttributes
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 3)
                    return null;
                var genericType = ad.ConstructorArguments[0].Value as INamedTypeSymbol;

                if (genericType is not { IsGenericType: true, IsUnboundGenericType: true } 
                    || ad.ConstructorArguments[1].Value is not string nameOfGenericParameter) 
                    return null;

                var typeParameterSymbol = genericType
                    .OriginalDefinition
                    .TypeArguments
                    .OfType<ITypeParameterSymbol>()
                    .FirstOrDefault(tps => tps.Name == nameOfGenericParameter);

                var substitutes = ad
                    .ConstructorArguments[2]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<INamedTypeSymbol>()
                    .ToList();
                
                return typeParameterSymbol is null 
                    ? null 
                    : ((INamedTypeSymbol, ITypeParameterSymbol, IReadOnlyList<INamedTypeSymbol>)?) (genericType, typeParameterSymbol, substitutes);
            })
            .OfType<(INamedTypeSymbol, ITypeParameterSymbol, IReadOnlyList<INamedTypeSymbol>)>());
        
        GenericParameterChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.GenericParameterChoiceAttribute, out var genericParameterChoiceAttributes) 
                ? genericParameterChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 3)
                    return null;
                var genericType = ad.ConstructorArguments[0].Value as INamedTypeSymbol;
                var typeChoice = ad.ConstructorArguments[2].Value as INamedTypeSymbol;

                if (genericType is not { IsGenericType: true, IsUnboundGenericType: true } 
                    || ad.ConstructorArguments[1].Value is not string nameOfGenericParameter
                    || typeChoice is null) 
                    return null;

                var typeParameterSymbol = genericType
                    .OriginalDefinition
                    .TypeArguments
                    .OfType<ITypeParameterSymbol>()
                    .FirstOrDefault(tps => tps.Name == nameOfGenericParameter);
                
                return typeParameterSymbol is null 
                    ? null 
                    : ((INamedTypeSymbol, ITypeParameterSymbol, INamedTypeSymbol)?) (genericType, typeParameterSymbol, typeChoice);
            })
            .OfType<(INamedTypeSymbol, ITypeParameterSymbol, INamedTypeSymbol)>());
        
        FilterGenericParameterSubstitutesChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterGenericParameterSubstitutesChoiceAttribute, out var filterGenericParameterSubstitutesChoiceAttributes) 
                ? filterGenericParameterSubstitutesChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 2)
                    return null;
                var genericType = ad.ConstructorArguments[0].Value as INamedTypeSymbol;

                if (genericType is not { IsGenericType: true, IsUnboundGenericType: true } 
                    || ad.ConstructorArguments[1].Value is not string nameOfGenericParameter) 
                    return null;

                var typeParameterSymbol = genericType
                    .OriginalDefinition
                    .TypeArguments
                    .OfType<ITypeParameterSymbol>()
                    .FirstOrDefault(tps => tps.Name == nameOfGenericParameter);
                
                return typeParameterSymbol is null 
                    ? null 
                    : ((INamedTypeSymbol, ITypeParameterSymbol)?) (genericType, typeParameterSymbol);
            })
            .OfType<(INamedTypeSymbol, ITypeParameterSymbol)>());
        
        FilterGenericParameterChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterGenericParameterChoiceAttribute, out var filterGenericParameterChoiceAttributes) 
                ? filterGenericParameterChoiceAttributes
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 2)
                    return null;
                var genericType = ad.ConstructorArguments[0].Value as INamedTypeSymbol;

                if (genericType is not { IsGenericType: true, IsUnboundGenericType: true } 
                    || ad.ConstructorArguments[1].Value is not string nameOfGenericParameter) 
                    return null;

                var typeParameterSymbol = genericType
                    .OriginalDefinition
                    .TypeArguments
                    .OfType<ITypeParameterSymbol>()
                    .FirstOrDefault(tps => tps.Name == nameOfGenericParameter);
                
                return typeParameterSymbol is null 
                    ? null 
                    : ((INamedTypeSymbol, ITypeParameterSymbol)?) (genericType, typeParameterSymbol);
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

        AssemblyImplementations = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesAggregation.AssemblyImplementationsAggregationAttribute, out var assemblyImplementationsAttributes)
                ? assemblyImplementationsAttributes
                : Enumerable.Empty<AttributeData>())
            .SelectMany(ad => ad.ConstructorArguments.Length < 1
                ? Array.Empty<IAssemblySymbol>()
                : ad.ConstructorArguments[0]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<INamedTypeSymbol>()
                    .Select(t => t.ContainingAssembly)
                    .OfType<IAssemblySymbol>())
            .Distinct(SymbolEqualityComparer.Default)
            .OfType<IAssemblySymbol>());

        FilterAssemblyImplementations = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesAggregation.FilterAssemblyImplementationsAggregationAttribute, out var filterAssemblyImplementationsAttributes)
                ? filterAssemblyImplementationsAttributes
                : Enumerable.Empty<AttributeData>())
            .SelectMany(ad => ad.ConstructorArguments.Length < 1
                ? Array.Empty<IAssemblySymbol>()
                : ad.ConstructorArguments[0]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<INamedTypeSymbol>()
                    .Select(t => t.ContainingAssembly)
                    .OfType<IAssemblySymbol>())
            .Distinct(SymbolEqualityComparer.Default)
            .OfType<IAssemblySymbol>());
        
        ImplementationChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.ImplementationChoiceAttribute, out var implementationChoiceAttributes) 
                ? implementationChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad => 
                ad.ConstructorArguments.Length < 2 
                || ad.ConstructorArguments[0].Value is not INamedTypeSymbol type 
                || ad.ConstructorArguments[1].Value is not INamedTypeSymbol implementation 
                    ? null 
                    : ((INamedTypeSymbol, INamedTypeSymbol)?) (type, implementation))
            .OfType<(INamedTypeSymbol, INamedTypeSymbol)>());
        
        ImplementationCollectionChoices = ImmutableHashSet.CreateRange(
            (AttributeDictionary.TryGetValue(wellKnownTypesChoice.ImplementationCollectionChoiceAttribute, out var implementationCollectionChoicesAttributes) 
                ? implementationCollectionChoicesAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 2)
                    return null;
                
                var type = ad.ConstructorArguments[0].Value as INamedTypeSymbol;
                
                var implementations = ad
                    .ConstructorArguments[1]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<INamedTypeSymbol>()
                    .ToList();
                
                return type is null 
                    ? null 
                    : ((INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)?) (type, implementations);
            })
            .OfType<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)>());
        
        FilterImplementationChoices = GetImplementationTypesFromAttribute(wellKnownTypesChoice.FilterImplementationChoiceAttribute);
        
        FilterImplementationCollectionChoices = GetImplementationTypesFromAttribute(wellKnownTypesChoice.FilterImplementationCollectionChoiceAttribute);
    }

    private IReadOnlyDictionary<ISymbol?, IGrouping<ISymbol?, AttributeData>> AttributeDictionary { get; }
    
    protected IImmutableSet<INamedTypeSymbol> GetAbstractionTypesFromAttribute(
        INamedTypeSymbol attribute)
    {
        return ImmutableHashSet.CreateRange(
            GetTypesFromAttribute(attribute)
                .Where(t =>
                {
                    var ret = _validateAttributes.ValidateAbstraction(t.Item2);

                    _warnings.Add(Diagnostics.ValidationConfigurationAttribute(
                        t.Item1,
                        _rangeType, 
                        _containerType, 
                        $"Given type \"{t.Item2.FullName()}\" isn't a valid abstraction type. It'll be ignored."));
                
                    return ret;
                })
                .Select(t => t.Item2));
    }
    
    protected IImmutableSet<INamedTypeSymbol> GetImplementationTypesFromAttribute(
        INamedTypeSymbol attribute)
    {
        return ImmutableHashSet.CreateRange(
            GetTypesFromAttribute(attribute)
            .Where(t =>
            {
                var ret = _validateAttributes.ValidateImplementation(t.Item2);

                _warnings.Add(Diagnostics.ValidationConfigurationAttribute(
                    t.Item1,
                    _rangeType, 
                    _containerType, 
                    $"Given type \"{t.Item2.FullName()}\" isn't a valid implementation type. It'll be ignored."));
                
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
    public IImmutableSet<INamedTypeSymbol> ContainerInstanceAbstraction { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> TransientScopeInstanceAbstraction { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> ScopeInstanceAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> TransientScopeRootAbstraction { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> ScopeRootAbstraction { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> DecoratorAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> CompositeAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> TransientImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> SyncTransientImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> AsyncTransientImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> ContainerInstanceImplementation { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> TransientScopeInstanceImplementation { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> ScopeInstanceImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> TransientScopeRootImplementation { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> ScopeRootImplementation { get; protected init; }
    public IImmutableSet<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> DecoratorSequenceChoices { get; }
    public IImmutableSet<(INamedTypeSymbol, IMethodSymbol)> ConstructorChoices { get; }
    public IImmutableSet<(INamedTypeSymbol, IMethodSymbol)> TypeInitializers { get; }
    public IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol, IReadOnlyList<INamedTypeSymbol>)> GenericParameterSubstitutesChoices { get; }
    public IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol, INamedTypeSymbol)> GenericParameterChoices { get; }
    public IImmutableSet<(INamedTypeSymbol, IReadOnlyList<IPropertySymbol>)> PropertyChoices { get; }
    public bool AllImplementations { get; }
    public IImmutableSet<IAssemblySymbol> AssemblyImplementations { get; }
    public IImmutableSet<(INamedTypeSymbol, INamedTypeSymbol)> ImplementationChoices { get; }
    public IImmutableSet<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> ImplementationCollectionChoices { get; }
    public IImmutableSet<INamedTypeSymbol> FilterImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> FilterTransientAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> FilterSyncTransientAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> FilterAsyncTransientAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> FilterContainerInstanceAbstraction { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> FilterTransientScopeInstanceAbstraction { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> FilterScopeInstanceAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> FilterTransientScopeRootAbstraction { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> FilterScopeRootAbstraction { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> FilterDecoratorAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> FilterCompositeAbstraction { get; }
    public IImmutableSet<INamedTypeSymbol> FilterTransientImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> FilterSyncTransientImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> FilterAsyncTransientImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> FilterContainerInstanceImplementation { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> FilterTransientScopeInstanceImplementation { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> FilterScopeInstanceImplementation { get; }
    public IImmutableSet<INamedTypeSymbol> FilterTransientScopeRootImplementation { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> FilterScopeRootImplementation { get; protected init; }
    public IImmutableSet<INamedTypeSymbol> FilterDecoratorSequenceChoices { get; }
    public IImmutableSet<INamedTypeSymbol> FilterConstructorChoices { get; }
    public IImmutableSet<INamedTypeSymbol> FilterTypeInitializers { get; }
    public IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol)> FilterGenericParameterSubstitutesChoices { get; }
    public IImmutableSet<(INamedTypeSymbol, ITypeParameterSymbol)> FilterGenericParameterChoices { get; }
    public IImmutableSet<INamedTypeSymbol> FilterPropertyChoices { get; }
    public bool FilterAllImplementations { get; }
    public IImmutableSet<IAssemblySymbol> FilterAssemblyImplementations { get; }
    public IImmutableSet<INamedTypeSymbol> FilterImplementationChoices { get; }
    public IImmutableSet<INamedTypeSymbol> FilterImplementationCollectionChoices { get; }

    public IReadOnlyList<Diagnostic> Warnings => _warnings;

    public IReadOnlyList<Diagnostic> Errors => _errors;
}