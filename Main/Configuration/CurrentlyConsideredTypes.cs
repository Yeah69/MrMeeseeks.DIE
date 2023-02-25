using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Configuration;

internal interface ICurrentlyConsideredTypes
{
    IImmutableSet<INamedTypeSymbol> AllConsideredImplementations { get; }
    IImmutableSet<INamedTypeSymbol> SyncTransientTypes { get; }
    IImmutableSet<INamedTypeSymbol> AsyncTransientTypes { get; }
    IImmutableSet<INamedTypeSymbol> ContainerInstanceTypes { get; }
    IImmutableSet<INamedTypeSymbol> TransientScopeInstanceTypes { get; }
    IImmutableSet<INamedTypeSymbol> ScopeInstanceTypes { get; }
    IImmutableSet<INamedTypeSymbol> TransientScopeRootTypes { get; }
    IImmutableSet<INamedTypeSymbol> ScopeRootTypes { get; }
    IImmutableSet<INamedTypeSymbol> DecoratorTypes { get; }
    IImmutableSet<INamedTypeSymbol> CompositeTypes { get; }
    IReadOnlyDictionary<ISymbol?, INamedTypeSymbol> InterfaceToComposite { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IMethodSymbol> ImplementationToConstructorChoice { get; }
    IReadOnlyDictionary<ISymbol?, IReadOnlyList<INamedTypeSymbol>> InterfaceToDecorators { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>>> DecoratorSequenceChoices { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IImmutableSet<INamedTypeSymbol>> ImplementationMap { get; }
    IReadOnlyDictionary<INamedTypeSymbol, (INamedTypeSymbol, IMethodSymbol)> ImplementationToInitializer { get; }
    IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), IReadOnlyList<INamedTypeSymbol>> GenericParameterSubstitutesChoices { get; }
    IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), INamedTypeSymbol> GenericParameterChoices { get; } 
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<string>> PropertyChoices { get; }
    IReadOnlyDictionary<INamedTypeSymbol, INamedTypeSymbol> ImplementationChoices { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> ImplementationCollectionChoices { get; }
}

internal class CurrentlyConsideredTypes : ICurrentlyConsideredTypes, ITransientScopeInstance
{
    public CurrentlyConsideredTypes(
        IReadOnlyList<ITypesFromAttributesBase> typesFromAttributes,
        IImplementationTypeSetCache implementationTypeSetCache)
    {
        IImmutableSet<INamedTypeSymbol> allImplementations = ImmutableHashSet<INamedTypeSymbol>.Empty;

        foreach (var types in typesFromAttributes)
        {
            if (types.FilterAllImplementations)
                allImplementations = ImmutableHashSet<INamedTypeSymbol>.Empty;
            else
            {
                allImplementations = allImplementations.Except(
                    types.FilterImplementation);
                allImplementations = types.FilterAssemblyImplementations.Aggregate(
                    allImplementations, 
                    (current, assembly) => current.Except(implementationTypeSetCache.ForAssembly(assembly)));
            }

            if (types.AllImplementations)
            {
                allImplementations = implementationTypeSetCache.All;
            }
            else
            {
                allImplementations = allImplementations.Union(
                    types.Implementation);
                allImplementations = types.AssemblyImplementations.Aggregate(
                    allImplementations, 
                    (current, assembly) => current.Union(implementationTypeSetCache.ForAssembly(assembly)));
            }
        }

        AllConsideredImplementations = ImmutableHashSet.CreateRange<INamedTypeSymbol>(
            CustomSymbolEqualityComparer.Default,
            allImplementations.Select(t => t.UnboundIfGeneric()));
        
        ImplementationMap = allImplementations
            .SelectMany(i => { return i.AllDerivedTypesAndSelf().Select(ii => (ii, i)); })
            .GroupBy<(INamedTypeSymbol, INamedTypeSymbol), INamedTypeSymbol, INamedTypeSymbol>(
                t => t.Item1.UnboundIfGeneric(), 
                t => t.Item2, 
                CustomSymbolEqualityComparer.Default)
            .ToDictionary<IGrouping<INamedTypeSymbol, INamedTypeSymbol>, INamedTypeSymbol, IImmutableSet<INamedTypeSymbol>>(
                g => g.Key, 
                g => ImmutableHashSet.CreateRange<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default, g),
                CustomSymbolEqualityComparer.Default);
        
        var transientTypes = GetSetOfTypesWithProperties(
            t => t.TransientAbstraction,
            t => t.FilterTransientAbstraction,
            t => t.TransientImplementation,
            t => t.FilterTransientImplementation,
            ImplementationMap);
        
        var syncTransientTypes = GetSetOfTypesWithProperties(
            t => t.SyncTransientAbstraction,
            t => t.FilterSyncTransientAbstraction,
            t => t.SyncTransientImplementation,
            t => t.FilterSyncTransientImplementation,
            ImplementationMap);
        
        var asyncTransientTypes = GetSetOfTypesWithProperties(
            t => t.AsyncTransientAbstraction,
            t => t.FilterAsyncTransientAbstraction,
            t => t.AsyncTransientImplementation,
            t => t.FilterAsyncTransientImplementation,
            ImplementationMap);

        SyncTransientTypes = syncTransientTypes.Union(transientTypes);

        AsyncTransientTypes = asyncTransientTypes.Union(transientTypes);
        
        ContainerInstanceTypes = GetSetOfTypesWithProperties(
            t => t.ContainerInstanceAbstraction,
            t => t.FilterContainerInstanceAbstraction,
            t => t.ContainerInstanceImplementation,
            t => t.FilterContainerInstanceImplementation,
            ImplementationMap);
        
        TransientScopeInstanceTypes = GetSetOfTypesWithProperties(
            t => t.TransientScopeInstanceAbstraction,
            t => t.FilterTransientScopeInstanceAbstraction,
            t => t.TransientScopeInstanceImplementation,
            t => t.FilterTransientScopeInstanceImplementation,
            ImplementationMap);
        
        ScopeInstanceTypes = GetSetOfTypesWithProperties(
            t => t.ScopeInstanceAbstraction,
            t => t.FilterScopeInstanceAbstraction,
            t => t.ScopeInstanceImplementation,
            t => t.FilterScopeInstanceImplementation,
            ImplementationMap);
        
        TransientScopeRootTypes = GetSetOfTypesWithProperties(
            t => t.TransientScopeRootAbstraction,
            t => t.FilterTransientScopeRootAbstraction,
            t => t.TransientScopeRootImplementation,
            t => t.FilterTransientScopeRootImplementation,
            ImplementationMap);
        
        ScopeRootTypes = GetSetOfTypesWithProperties(
            t => t.ScopeRootAbstraction,
            t => t.FilterScopeRootAbstraction,
            t => t.ScopeRootImplementation,
            t => t.FilterScopeRootImplementation,
            ImplementationMap);
        
        var compositeInterfaces = ImmutableHashSet<INamedTypeSymbol>.Empty;
        foreach (var types in typesFromAttributes)
        {
            compositeInterfaces = compositeInterfaces.Except(types.FilterCompositeAbstraction.Select(c => c.UnboundIfGeneric()));
            compositeInterfaces = compositeInterfaces.Union(types.CompositeAbstraction.Select(c => c.UnboundIfGeneric()));
        }
        
        CompositeTypes = GetSetOfTypesWithProperties(
            t => t.CompositeAbstraction, 
            t => t.FilterCompositeAbstraction,
            _ => ImmutableHashSet<INamedTypeSymbol>.Empty, 
            _ => ImmutableHashSet<INamedTypeSymbol>.Empty,
            ImplementationMap);
        
        InterfaceToComposite = CompositeTypes
            .GroupBy(nts =>
            {
                var namedTypeSymbol = nts.OriginalDefinition.AllInterfaces
                    .Single(t => compositeInterfaces.Contains(t.UnboundIfGeneric(), CustomSymbolEqualityComparer.Default));
                return namedTypeSymbol.TypeArguments.FirstOrDefault() is INamedTypeSymbol interfaceTypeSymbol
                    ? interfaceTypeSymbol.UnboundIfGeneric()
                    : throw new ValidationDieException(ImmutableList.Create(Diagnostics.ValidationGeneral("Composite should implement composite interface")));
            }, CustomSymbolEqualityComparer.Default)
            .Where(g => g.Count() == 1)
            .ToDictionary(g => g.Key, g => g.Single(), CustomSymbolEqualityComparer.Default);

        var constructorChoices = new Dictionary<INamedTypeSymbol, IMethodSymbol>(CustomSymbolEqualityComparer.Default);
        
        foreach (var types in typesFromAttributes)
        {
            foreach (var filterConstructorChoice in types.FilterConstructorChoices)
                constructorChoices.Remove(filterConstructorChoice);

            foreach (var (implementationType, constructor) in types.ConstructorChoices)
                constructorChoices[implementationType.UnboundIfGeneric()] = constructor;
        }
        
        ImplementationToConstructorChoice = constructorChoices;

        var propertyChoices = new Dictionary<INamedTypeSymbol, IReadOnlyList<string>>(CustomSymbolEqualityComparer.Default);
        
        foreach (var types in typesFromAttributes)
        {
            foreach (var filterPropertyChoice in types.FilterPropertyChoices)
                propertyChoices.Remove(filterPropertyChoice);

            foreach (var (implementationType, properties) in types.PropertyChoices)
                propertyChoices[implementationType.UnboundIfGeneric()] = properties;
        }
        
        PropertyChoices = propertyChoices;

        var decoratorInterfaces = ImmutableHashSet<INamedTypeSymbol>.Empty;
        foreach (var types in typesFromAttributes)
        {
            decoratorInterfaces = decoratorInterfaces.Except(types.FilterDecoratorAbstraction.Select(c => c.UnboundIfGeneric()));
            decoratorInterfaces = decoratorInterfaces.Union(types.DecoratorAbstraction.Select(c => c.UnboundIfGeneric()));
        }
        
        DecoratorTypes = GetSetOfTypesWithProperties(
            t => t.DecoratorAbstraction, 
            t => t.FilterDecoratorAbstraction,
            _ => ImmutableHashSet<INamedTypeSymbol>.Empty, 
            _ => ImmutableHashSet<INamedTypeSymbol>.Empty,
            ImplementationMap);
        
        InterfaceToDecorators = DecoratorTypes
            .GroupBy(nts =>
            {
                var namedTypeSymbol = nts.OriginalDefinition.AllInterfaces
                    .Single(t => decoratorInterfaces.Contains(t.UnboundIfGeneric(), CustomSymbolEqualityComparer.Default));
                return namedTypeSymbol.TypeArguments.FirstOrDefault() is INamedTypeSymbol interfaceTypeSymbol
                    ? interfaceTypeSymbol.UnboundIfGeneric()
                    : throw new ValidationDieException(ImmutableList.Create(Diagnostics.ValidationGeneral("Decorator should implement decorator interface")));
            }, CustomSymbolEqualityComparer.Default)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<INamedTypeSymbol>) g.ToList(), CustomSymbolEqualityComparer.Default);
        
        var decoratorSequenceChoices = new Dictionary<INamedTypeSymbol, DecoratorSequenceMap>(CustomSymbolEqualityComparer.Default);
        
        foreach (var types in typesFromAttributes)
        {
            foreach (var (interfaceType, decoratedType) in types.FilterDecoratorSequenceChoices)
                if (decoratorSequenceChoices.TryGetValue(interfaceType, out var sequenceMap))
                    sequenceMap.Remove(decoratedType);

            foreach (var (interfaceType, decoratedType, decoratorSequence) in types.DecoratorSequenceChoices)
            {
                if (!decoratorSequenceChoices.TryGetValue(interfaceType, out var sequenceMap))
                {
                    sequenceMap = new DecoratorSequenceMap();
                    decoratorSequenceChoices[interfaceType] = sequenceMap;
                }
                sequenceMap.Add(decoratedType, decoratorSequence);
            }
        }

        DecoratorSequenceChoices = decoratorSequenceChoices
            .Where(kvp => kvp.Value.Any)
            .ToDictionary<KeyValuePair<INamedTypeSymbol, DecoratorSequenceMap>, INamedTypeSymbol, IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>>>(
                kvp => kvp.Key, 
                kvp => kvp.Value.ToReadOnlyDictionary(),
                CustomSymbolEqualityComparer.Default);

        var initializers = new Dictionary<INamedTypeSymbol, (INamedTypeSymbol, IMethodSymbol)>(CustomSymbolEqualityComparer.Default);
        
        foreach (var types in typesFromAttributes)
        {
            var filterInterfaceTypes = types
                .FilterInitializers
                .Where(t => t is { TypeKind: TypeKind.Interface} or { TypeKind: not TypeKind.Interface, IsAbstract: true});

            foreach (var interfaceType in filterInterfaceTypes)
                if (ImplementationMap.TryGetValue(interfaceType.UnboundIfGeneric(), out var set))
                    foreach (var concreteType in set)
                        initializers.Remove(concreteType.UnboundIfGeneric());

            var filterConcreteTypes = types
                .FilterInitializers
                .Where(t => t is { TypeKind: TypeKind.Class or TypeKind.Struct, IsAbstract: false })
                .ToList();
            
            foreach (var filterConcreteType in filterConcreteTypes)
                initializers.Remove(filterConcreteType.UnboundIfGeneric());
            
            var interfaceTypes = types
                .Initializers
                .Where(t => t.Item1 is { TypeKind: TypeKind.Interface} or { TypeKind: not TypeKind.Interface, IsAbstract: true})
                .ToList();
            
            foreach (var tuple in interfaceTypes)
                if (ImplementationMap.TryGetValue(tuple.Item1.UnboundIfGeneric(), out var set))
                    foreach (var concreteType in set)
                        initializers[concreteType.UnboundIfGeneric()] = (tuple.Item1.UnboundIfGeneric(), tuple.Item2);

            var concreteTypes = types
                .Initializers
                .Where(t => t.Item1 is { TypeKind: TypeKind.Class or TypeKind.Struct, IsAbstract: false })
                .ToList();
            
            foreach (var (implementation, initializer) in concreteTypes)
                initializers[implementation.UnboundIfGeneric()] = (implementation.UnboundIfGeneric(), initializer);
        }

        ImplementationToInitializer = initializers;

        var genericParameterSubstitutes =
            new Dictionary<(INamedTypeSymbol, ITypeParameterSymbol), IReadOnlyList<INamedTypeSymbol>>();
        
        foreach (var typesFromAttribute in typesFromAttributes)
        {
            foreach (var tuple in typesFromAttribute.FilterGenericParameterSubstitutesChoices)
                genericParameterSubstitutes.Remove(tuple);

            foreach (var tuple in typesFromAttribute.GenericParameterSubstitutesChoices)
            {
                var key = (tuple.Item1, tuple.Item2);
                var choice = tuple.Item3;

                genericParameterSubstitutes[key] = choice;
            }
        }

        GenericParameterSubstitutesChoices = genericParameterSubstitutes;

        var genericParameterChoices =
            new Dictionary<(INamedTypeSymbol, ITypeParameterSymbol), INamedTypeSymbol>();
        
        foreach (var typesFromAttribute in typesFromAttributes)
        {
            foreach (var tuple in typesFromAttribute.FilterGenericParameterChoices)
                genericParameterChoices.Remove((tuple.Item1, tuple.Item2));

            foreach (var tuple in typesFromAttribute.GenericParameterChoices)
                genericParameterChoices[(tuple.Item1, tuple.Item2)] = tuple.Item3;
        }

        GenericParameterChoices = genericParameterChoices;


        var implementationChoices =
            new Dictionary<INamedTypeSymbol, INamedTypeSymbol>(CustomSymbolEqualityComparer.Default);
        
        foreach (var typesFromAttribute in typesFromAttributes)
        {
            foreach (var type in typesFromAttribute.FilterImplementationChoices)
                implementationChoices.Remove(type);

            foreach (var (type, choice) in typesFromAttribute.ImplementationChoices)
                implementationChoices[type.UnboundIfGeneric()] = choice;
        }

        ImplementationChoices = implementationChoices;

        var implementationCollectionChoices =
            new Dictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>>(CustomSymbolEqualityComparer.Default);
        
        foreach (var typesFromAttribute in typesFromAttributes)
        {
            foreach (var type in typesFromAttribute.FilterImplementationCollectionChoices)
                implementationCollectionChoices.Remove(type);

            foreach (var (type, choice) in typesFromAttribute.ImplementationCollectionChoices)
                implementationCollectionChoices[type.UnboundIfGeneric()] = choice;
        }

        ImplementationCollectionChoices = implementationCollectionChoices;
        
        IImmutableSet<INamedTypeSymbol> GetSetOfTypesWithProperties(
            Func<ITypesFromAttributesBase, IImmutableSet<INamedTypeSymbol>> propertyGivingAbstractTypesGetter, 
            Func<ITypesFromAttributesBase, IImmutableSet<INamedTypeSymbol>> filteredPropertyGivingAbstractTypesGetter,
            Func<ITypesFromAttributesBase, IImmutableSet<INamedTypeSymbol>> propertyGivingImplementationTypesGetter, 
            Func<ITypesFromAttributesBase, IImmutableSet<INamedTypeSymbol>> filteredPropertyGivingImplementationTypesGetter,
            IReadOnlyDictionary<INamedTypeSymbol, IImmutableSet<INamedTypeSymbol>> implementationMap)
        {
            var ret = ImmutableHashSet<INamedTypeSymbol>.Empty;
            
            foreach (var types in typesFromAttributes)
            {
                var filteredTypes = ImmutableHashSet.CreateRange<INamedTypeSymbol>(
                    CustomSymbolEqualityComparer.Default,
                    filteredPropertyGivingImplementationTypesGetter(types).Select(t => t.UnboundIfGeneric()));
                foreach (var type in filteredPropertyGivingAbstractTypesGetter(types))
                    if (implementationMap.TryGetValue(type.UnboundIfGeneric(), out var set)) 
                        filteredTypes = filteredTypes.Union(set.Select(t => t.UnboundIfGeneric()));
                
                ret = ret.Except(filteredTypes);
                
                var addedTypes = ImmutableHashSet.CreateRange<INamedTypeSymbol>(
                    CustomSymbolEqualityComparer.Default,
                    propertyGivingImplementationTypesGetter(types).Select(t => t.UnboundIfGeneric()));
                foreach (var type in propertyGivingAbstractTypesGetter(types))
                    if (implementationMap.TryGetValue(type.UnboundIfGeneric(), out var set)) 
                        addedTypes = addedTypes.Union(set.Select(t => t.UnboundIfGeneric()));
                
                ret = ret.Union(addedTypes);
            }
            
            return ret;
        }
    }

    public IImmutableSet<INamedTypeSymbol> AllConsideredImplementations { get; }
    public IImmutableSet<INamedTypeSymbol> SyncTransientTypes { get; }
    public IImmutableSet<INamedTypeSymbol> AsyncTransientTypes { get; }
    public IImmutableSet<INamedTypeSymbol> ContainerInstanceTypes { get; }
    public IImmutableSet<INamedTypeSymbol> TransientScopeInstanceTypes { get; }
    public IImmutableSet<INamedTypeSymbol> ScopeInstanceTypes { get; }
    public IImmutableSet<INamedTypeSymbol> TransientScopeRootTypes { get; }
    public IImmutableSet<INamedTypeSymbol> ScopeRootTypes { get; }
    public IImmutableSet<INamedTypeSymbol> DecoratorTypes { get; }
    public IImmutableSet<INamedTypeSymbol> CompositeTypes { get; }
    public IReadOnlyDictionary<ISymbol?, INamedTypeSymbol> InterfaceToComposite { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IMethodSymbol> ImplementationToConstructorChoice { get; }
    public IReadOnlyDictionary<ISymbol?, IReadOnlyList<INamedTypeSymbol>> InterfaceToDecorators { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>>> DecoratorSequenceChoices { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IImmutableSet<INamedTypeSymbol>> ImplementationMap { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, (INamedTypeSymbol, IMethodSymbol)> ImplementationToInitializer { get; }
    public IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), IReadOnlyList<INamedTypeSymbol>> GenericParameterSubstitutesChoices { get; }
    public IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), INamedTypeSymbol> GenericParameterChoices { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<string>> PropertyChoices { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, INamedTypeSymbol> ImplementationChoices { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> ImplementationCollectionChoices { get; }

    private class DecoratorSequenceMap
    {
        private readonly Dictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> _map = new(CustomSymbolEqualityComparer.Default);

        public void Add(INamedTypeSymbol decoratedType, IReadOnlyList<INamedTypeSymbol> decoratorSequence) => 
            _map[decoratedType] = decoratorSequence;

        public void Remove(INamedTypeSymbol decoratedType) =>
            _map.Remove(decoratedType);

        public bool Any => 
            _map.Any();

        public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> ToReadOnlyDictionary() => 
            _map;
    }
}