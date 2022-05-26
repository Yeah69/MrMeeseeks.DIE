using MrMeeseeks.DIE.Extensions;

namespace MrMeeseeks.DIE.Configuration;

internal interface ITypesFromAttributes
{
    IReadOnlyList<INamedTypeSymbol> Implementation { get; }
    IReadOnlyList<INamedTypeSymbol> TransientAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> SyncTransientAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> AsyncTransientAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> ContainerInstanceAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> TransientScopeInstanceAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> ScopeInstance { get; }
    IReadOnlyList<INamedTypeSymbol> TransientScopeRootAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> ScopeRootAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> DecoratorAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> CompositeAbstraction { get; }
    IReadOnlyList<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> DecoratorSequenceChoices { get; }
    IReadOnlyList<(INamedTypeSymbol, IMethodSymbol)> ConstructorChoices { get; }
    IReadOnlyList<(INamedTypeSymbol, IMethodSymbol)> TypeInitializers { get; }
    IReadOnlyList<(INamedTypeSymbol, ITypeParameterSymbol, IReadOnlyList<INamedTypeSymbol>)> GenericParameterSubstitutesChoices { get; } 
    IReadOnlyList<(INamedTypeSymbol, ITypeParameterSymbol, INamedTypeSymbol)> GenericParameterChoices { get; } 
    IReadOnlyList<(INamedTypeSymbol, IReadOnlyList<IPropertySymbol>)> PropertyChoices { get; }
    bool AllImplementations { get; }
    IReadOnlyList<IAssemblySymbol> AssemblyImplementations { get; }
    IReadOnlyList<(INamedTypeSymbol, INamedTypeSymbol)> ImplementationChoices { get; }
    IReadOnlyList<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> ImplementationCollectionChoices { get; }
    IReadOnlyList<INamedTypeSymbol> FilterImplementation { get; }
    IReadOnlyList<INamedTypeSymbol> FilterTransientAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> FilterSyncTransientAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> FilterAsyncTransientAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> FilterContainerInstanceAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> FilterTransientScopeInstanceAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> FilterScopeInstanceAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> FilterTransientScopeRootAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> FilterScopeRootAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> FilterDecoratorAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> FilterCompositeAbstraction { get; }
    IReadOnlyList<INamedTypeSymbol> FilterDecoratorSequenceChoices { get; }
    IReadOnlyList<INamedTypeSymbol> FilterConstructorChoices { get; }
    IReadOnlyList<INamedTypeSymbol> FilterTypeInitializers { get; }
    IReadOnlyList<(INamedTypeSymbol, ITypeParameterSymbol)> FilterGenericParameterSubstitutesChoices { get; } 
    IReadOnlyList<(INamedTypeSymbol, ITypeParameterSymbol)> FilterGenericParameterChoices { get; }
    IReadOnlyList<INamedTypeSymbol> FilterPropertyChoices { get; }
    bool FilterAllImplementations { get; }
    IReadOnlyList<IAssemblySymbol> FilterAssemblyImplementations { get; }
    IReadOnlyList<INamedTypeSymbol> FilterImplementationChoices { get; }
    IReadOnlyList<INamedTypeSymbol> FilterImplementationCollectionChoices { get; }
}

internal class TypesFromAttributes : ScopeTypesFromAttributes
{
    internal TypesFromAttributes(
        // parameter
        IReadOnlyList<AttributeData> attributeData,

        // dependencies
        WellKnownTypesAggregation wellKnownTypesAggregation,
        WellKnownTypesChoice wellKnownTypesChoice,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous) 
        : base(attributeData, wellKnownTypesAggregation, wellKnownTypesChoice, wellKnownTypesMiscellaneous)
    {
        ContainerInstanceAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.ContainerInstanceAbstractionAggregationAttribute).ToList();
        TransientScopeInstanceAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.TransientScopeInstanceAbstractionAggregationAttribute).ToList();
        TransientScopeRootAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.TransientScopeRootAbstractionAggregationAttribute).ToList();
        ScopeRootAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.ScopeRootAbstractionAggregationAttribute).ToList();
        FilterContainerInstanceAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.FilterContainerInstanceAbstractionAggregationAttribute).ToList();
        FilterTransientScopeInstanceAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.FilterTransientScopeInstanceAbstractionAggregationAttribute).ToList();
        FilterTransientScopeRootAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.FilterTransientScopeRootAbstractionAggregationAttribute).ToList();
        FilterScopeRootAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.FilterScopeRootAbstractionAggregationAttribute).ToList();
    }
}

internal class ScopeTypesFromAttributes : ITypesFromAttributes
{
    internal ScopeTypesFromAttributes(
        // parameter
        IReadOnlyList<AttributeData> attributeData,

        // dependencies
        WellKnownTypesAggregation wellKnownTypesAggregation,
        WellKnownTypesChoice wellKnownTypesChoice,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous)
    {
        AttributeDictionary = attributeData
            .GroupBy(ad => ad.AttributeClass, SymbolEqualityComparer.Default)
            .ToDictionary(g => g.Key, g => g);

        Implementation = GetTypesFromAttribute(wellKnownTypesAggregation.ImplementationAggregationAttribute).ToList();
        TransientAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.TransientAbstractionAggregationAttribute).ToList();
        SyncTransientAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.SyncTransientAbstractionAggregationAttribute).ToList();
        AsyncTransientAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.AsyncTransientAbstractionAggregationAttribute).ToList();
        ContainerInstanceAbstraction = new List<INamedTypeSymbol>();
        TransientScopeInstanceAbstraction = new List<INamedTypeSymbol>();
        ScopeInstance = GetTypesFromAttribute(wellKnownTypesAggregation.ScopeInstanceAbstractionAggregationAttribute).ToList();
        TransientScopeRootAbstraction = new List<INamedTypeSymbol>();
        ScopeRootAbstraction = new List<INamedTypeSymbol>();
        DecoratorAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.DecoratorAbstractionAggregationAttribute).ToList();
        CompositeAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.CompositeAbstractionAggregationAttribute).ToList();

        FilterImplementation = GetTypesFromAttribute(wellKnownTypesAggregation.FilterImplementationAggregationAttribute).ToList();
        FilterTransientAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.FilterTransientAbstractionAggregationAttribute).ToList();
        FilterSyncTransientAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.FilterSyncTransientAbstractionAggregationAttribute).ToList();
        FilterAsyncTransientAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.FilterAsyncTransientAbstractionAggregationAttribute).ToList();
        FilterContainerInstanceAbstraction = new List<INamedTypeSymbol>();
        FilterTransientScopeInstanceAbstraction = new List<INamedTypeSymbol>();
        FilterScopeInstanceAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.FilterScopeInstanceAbstractionAggregationAttribute).ToList();
        FilterTransientScopeRootAbstraction = new List<INamedTypeSymbol>();
        FilterScopeRootAbstraction = new List<INamedTypeSymbol>();
        FilterDecoratorAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.FilterDecoratorAbstractionAggregationAttribute).ToList();
        FilterCompositeAbstraction = GetTypesFromAttribute(wellKnownTypesAggregation.FilterCompositeAbstractionAggregationAttribute).ToList();
        
        DecoratorSequenceChoices = (AttributeDictionary.TryGetValue(wellKnownTypesChoice.DecoratorSequenceChoiceAttribute, out var decoratorSequenceChoiceAttributes) 
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
            .OfType<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)>()
            .ToList();
        
        FilterDecoratorSequenceChoices = (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterDecoratorSequenceChoiceAttribute, out var filterDecoratorSequenceChoiceAttributes) 
                ? filterDecoratorSequenceChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length != 1)
                    return null;
                
                return ad.ConstructorArguments[0].Value as INamedTypeSymbol;
            })
            .OfType<INamedTypeSymbol>()
            .ToList();

        ConstructorChoices = (AttributeDictionary.TryGetValue(wellKnownTypesChoice.ConstructorChoiceAttribute, out var constructorChoiceAttributes) 
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
                    .Constructors
                    .Where(c => c.Parameters.Length == parameterTypes.Count)
                    .SingleOrDefault(c => c.Parameters.Select(p => p.Type)
                        .Zip(parameterTypes,
                            (pLeft, pRight) => pLeft.Equals(pRight, SymbolEqualityComparer.Default))
                        .All(b => b));
                return constructorChoice is { }
                    ? (implementationType, constructorChoice)
                    : ((INamedTypeSymbol, IMethodSymbol)?)null;

            })
            .OfType<(INamedTypeSymbol, IMethodSymbol)>()
            .ToList();

        FilterConstructorChoices = (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterConstructorChoiceAttribute, out var filterConstructorChoiceAttributes) 
                ? filterConstructorChoiceAttributes
                : Enumerable.Empty<AttributeData>())
            .Select(ad => ad.ConstructorArguments.Length < 1 
                ? null 
                : ad.ConstructorArguments[0].Value as INamedTypeSymbol)
            .OfType<INamedTypeSymbol>()
            .ToList();

        PropertyChoices = (AttributeDictionary.TryGetValue(wellKnownTypesChoice.PropertyChoiceAttribute, out var propertyChoiceGroup) 
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
            .OfType<(INamedTypeSymbol, IReadOnlyList<IPropertySymbol>)>()
            .ToList();

        FilterPropertyChoices = (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterPropertyChoiceAttribute, out var filterPropertyChoicesGroup)
                ? filterPropertyChoicesGroup
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 1)
                    return null;
                return ad.ConstructorArguments[0].Value as INamedTypeSymbol;
            })
            .OfType<INamedTypeSymbol>()
            .ToList();

        TypeInitializers = (AttributeDictionary.TryGetValue(wellKnownTypesMiscellaneous.TypeInitializerAttribute, out var typeInitializerAttributes) 
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
            .OfType<(INamedTypeSymbol, IMethodSymbol)>()
            .ToList();
        
        FilterTypeInitializers = (AttributeDictionary.TryGetValue(wellKnownTypesMiscellaneous.FilterTypeInitializerAttribute, out var filterTypeInitializerAttributes) 
                ? filterTypeInitializerAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length != 1)
                    return null;
                return ad.ConstructorArguments[0].Value as INamedTypeSymbol;
            })
            .OfType<INamedTypeSymbol>()
            .ToList();
        
        GenericParameterSubstitutesChoices = (AttributeDictionary.TryGetValue(wellKnownTypesChoice.GenericParameterSubstitutesChoiceAttribute, out var genericParameterSubstitutesChoiceAttributes) 
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
            .OfType<(INamedTypeSymbol, ITypeParameterSymbol, IReadOnlyList<INamedTypeSymbol>)>()
            .ToList();
        
        GenericParameterChoices = (AttributeDictionary.TryGetValue(wellKnownTypesChoice.GenericParameterChoiceAttribute, out var genericParameterChoiceAttributes) 
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
            .OfType<(INamedTypeSymbol, ITypeParameterSymbol, INamedTypeSymbol)>()
            .ToList();
        
        FilterGenericParameterSubstitutesChoices = (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterGenericParameterSubstitutesChoiceAttribute, out var filterGenericParameterSubstitutesChoiceAttributes) 
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
            .OfType<(INamedTypeSymbol, ITypeParameterSymbol)>()
            .ToList();
        
        FilterGenericParameterChoices = (AttributeDictionary.TryGetValue(wellKnownTypesChoice.FilterGenericParameterChoiceAttribute, out var filterGenericParameterChoiceAttributes) 
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
            .OfType<(INamedTypeSymbol, ITypeParameterSymbol)>()
            .ToList();

        AllImplementations = (AttributeDictionary.TryGetValue(wellKnownTypesAggregation.AllImplementationsAggregationAttribute, out var allImplementationsAttributes)
                ? allImplementationsAttributes
                : Enumerable.Empty<AttributeData>())
            .Any();

        FilterAllImplementations = (AttributeDictionary.TryGetValue(wellKnownTypesAggregation.FilterAllImplementationsAggregationAttribute, out var filterAllImplementationsAttributes)
                ? filterAllImplementationsAttributes
                : Enumerable.Empty<AttributeData>())
            .Any();

        AssemblyImplementations = (AttributeDictionary.TryGetValue(wellKnownTypesAggregation.AssemblyImplementationsAggregationAttribute, out var assemblyImplementationsAttributes)
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
            .OfType<IAssemblySymbol>()
            .ToList();

        FilterAssemblyImplementations = (AttributeDictionary.TryGetValue(wellKnownTypesAggregation.FilterAllImplementationsAggregationAttribute, out var filterAssemblyImplementationsAttributes)
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
            .OfType<IAssemblySymbol>()
            .ToList();
        
        ImplementationChoices = (AttributeDictionary.TryGetValue(wellKnownTypesChoice.ImplementationChoiceAttribute, out var implementationChoiceAttributes) 
                ? implementationChoiceAttributes 
                : Enumerable.Empty<AttributeData>())
            .Select(ad => 
                ad.ConstructorArguments.Length < 2 
                || ad.ConstructorArguments[0].Value is not INamedTypeSymbol type 
                || ad.ConstructorArguments[1].Value is not INamedTypeSymbol implementation 
                    ? null 
                    : ((INamedTypeSymbol, INamedTypeSymbol)?) (type, implementation))
            .OfType<(INamedTypeSymbol, INamedTypeSymbol)>()
            .ToList();
        
        ImplementationCollectionChoices = (AttributeDictionary.TryGetValue(wellKnownTypesChoice.ImplementationCollectionChoiceAttribute, out var implementationCollectionChoicesAttributes) 
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
            .OfType<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)>()
            .ToList();
        
        FilterImplementationChoices = GetTypesFromAttribute(wellKnownTypesChoice.FilterImplementationChoiceAttribute).ToList();
        
        FilterImplementationCollectionChoices = GetTypesFromAttribute(wellKnownTypesChoice.FilterImplementationCollectionChoiceAttribute).ToList();
    }

    private IReadOnlyDictionary<ISymbol?, IGrouping<ISymbol?, AttributeData>> AttributeDictionary { get; }
    
    protected IEnumerable<INamedTypeSymbol> GetTypesFromAttribute(
        INamedTypeSymbol attribute)
    {
        return (AttributeDictionary.TryGetValue(attribute, out var attributes) ? attributes : Enumerable.Empty<AttributeData>())
            .SelectMany(ad => ad.ConstructorArguments
                .Where(tc => tc.Kind == TypedConstantKind.Type)
                .OfType<TypedConstant>()
                .Concat(ad.ConstructorArguments.SelectMany(ca => ca.Kind is TypedConstantKind.Array
                    ? (IEnumerable<TypedConstant>)ca.Values
                    : Array.Empty<TypedConstant>())))
            .Select(tc => !CheckValidType(tc, out var type) ? null : type)
            .Where(t => t is not null)
            .OfType<INamedTypeSymbol>()
            .Select(t => t.OriginalDefinition);
        
        bool CheckValidType(TypedConstant typedConstant, out INamedTypeSymbol type)
        {
            type = (typedConstant.Value as INamedTypeSymbol)!;
            return typedConstant.Value is not null;
        }
    }

    public IReadOnlyList<INamedTypeSymbol> Implementation { get; }
    public IReadOnlyList<INamedTypeSymbol> TransientAbstraction { get; }
    public IReadOnlyList<INamedTypeSymbol> SyncTransientAbstraction { get; }
    public IReadOnlyList<INamedTypeSymbol> AsyncTransientAbstraction { get; }
    public IReadOnlyList<INamedTypeSymbol> ContainerInstanceAbstraction { get; protected init; }
    public IReadOnlyList<INamedTypeSymbol> TransientScopeInstanceAbstraction { get; protected init; }
    public IReadOnlyList<INamedTypeSymbol> ScopeInstance { get; }
    public IReadOnlyList<INamedTypeSymbol> TransientScopeRootAbstraction { get; protected init; }
    public IReadOnlyList<INamedTypeSymbol> ScopeRootAbstraction { get; protected init; }
    public IReadOnlyList<INamedTypeSymbol> DecoratorAbstraction { get; }
    public IReadOnlyList<INamedTypeSymbol> CompositeAbstraction { get; }
    public IReadOnlyList<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> DecoratorSequenceChoices { get; }
    public IReadOnlyList<(INamedTypeSymbol, IMethodSymbol)> ConstructorChoices { get; }
    public IReadOnlyList<(INamedTypeSymbol, IMethodSymbol)> TypeInitializers { get; }
    public IReadOnlyList<(INamedTypeSymbol, ITypeParameterSymbol, IReadOnlyList<INamedTypeSymbol>)> GenericParameterSubstitutesChoices { get; }
    public IReadOnlyList<(INamedTypeSymbol, ITypeParameterSymbol, INamedTypeSymbol)> GenericParameterChoices { get; }
    public IReadOnlyList<(INamedTypeSymbol, IReadOnlyList<IPropertySymbol>)> PropertyChoices { get; }
    public bool AllImplementations { get; }
    public IReadOnlyList<IAssemblySymbol> AssemblyImplementations { get; }
    public IReadOnlyList<(INamedTypeSymbol, INamedTypeSymbol)> ImplementationChoices { get; }
    public IReadOnlyList<(INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>)> ImplementationCollectionChoices { get; }
    public IReadOnlyList<INamedTypeSymbol> FilterImplementation { get; }
    public IReadOnlyList<INamedTypeSymbol> FilterTransientAbstraction { get; }
    public IReadOnlyList<INamedTypeSymbol> FilterSyncTransientAbstraction { get; }
    public IReadOnlyList<INamedTypeSymbol> FilterAsyncTransientAbstraction { get; }
    public IReadOnlyList<INamedTypeSymbol> FilterContainerInstanceAbstraction { get; protected init; }
    public IReadOnlyList<INamedTypeSymbol> FilterTransientScopeInstanceAbstraction { get; protected init; }
    public IReadOnlyList<INamedTypeSymbol> FilterScopeInstanceAbstraction { get; }
    public IReadOnlyList<INamedTypeSymbol> FilterTransientScopeRootAbstraction { get; protected init; }
    public IReadOnlyList<INamedTypeSymbol> FilterScopeRootAbstraction { get; protected init; }
    public IReadOnlyList<INamedTypeSymbol> FilterDecoratorAbstraction { get; }
    public IReadOnlyList<INamedTypeSymbol> FilterCompositeAbstraction { get; }
    public IReadOnlyList<INamedTypeSymbol> FilterDecoratorSequenceChoices { get; }
    public IReadOnlyList<INamedTypeSymbol> FilterConstructorChoices { get; }
    public IReadOnlyList<INamedTypeSymbol> FilterTypeInitializers { get; }
    public IReadOnlyList<(INamedTypeSymbol, ITypeParameterSymbol)> FilterGenericParameterSubstitutesChoices { get; }
    public IReadOnlyList<(INamedTypeSymbol, ITypeParameterSymbol)> FilterGenericParameterChoices { get; }
    public IReadOnlyList<INamedTypeSymbol> FilterPropertyChoices { get; }
    public bool FilterAllImplementations { get; }
    public IReadOnlyList<IAssemblySymbol> FilterAssemblyImplementations { get; }
    public IReadOnlyList<INamedTypeSymbol> FilterImplementationChoices { get; }
    public IReadOnlyList<INamedTypeSymbol> FilterImplementationCollectionChoices { get; }
}