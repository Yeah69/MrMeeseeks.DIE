using System.Collections.Concurrent;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Configuration;

internal interface IContainerCurrentlyConsideredTypes : ICurrentlyConsideredTypes;

internal sealed class ContainerCurrentlyConsideredTypes : CurrentlyConsideredTypesBase, IContainerCurrentlyConsideredTypes, IContainerInstance
{
    internal ContainerCurrentlyConsideredTypes(
        IAssemblyTypesFromAttributes assemblyTypesFromAttributes,
        IContainerTypesFromAttributes containerTypesFromAttributes,
        ImplementationCache implementationCache)
    : base(
        [assemblyTypesFromAttributes, containerTypesFromAttributes],
        implementationCache)
    {
    }
}

internal interface IScopeCurrentlyConsideredTypes : ICurrentlyConsideredTypes;

internal sealed class ScopeCurrentlyConsideredTypes : CurrentlyConsideredTypesBase, IScopeCurrentlyConsideredTypes, ITransientScopeInstance
{
    internal ScopeCurrentlyConsideredTypes(
        IAssemblyTypesFromAttributes assemblyTypesFromAttributes,
        IContainerTypesFromAttributes containerTypesFromAttributes,
        IScopeTypesFromAttributes scopeTypesFromAttributes,
        ImplementationCache implementationCache)
        : base(
            [assemblyTypesFromAttributes, containerTypesFromAttributes, scopeTypesFromAttributes],
            implementationCache)
    {
    }
}

internal interface ICurrentlyConsideredTypes
{
    IImmutableSet<INamedTypeSymbol> AllConsideredImplementations { get; }
    IImmutableSet<INamedTypeSymbol> InjectionKeyAttributeTypes { get; }
    IImmutableSet<(ITypeSymbol KeyType, object KeyValue, INamedTypeSymbol ImplementationType)> InjectionKeyChoices { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>> ImplementationToConstructorChoice { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>>> DecoratorSequenceChoices { get; }
    IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), IReadOnlyList<INamedTypeSymbol>> GenericParameterSubstitutesChoices { get; }
    IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), INamedTypeSymbol> GenericParameterChoices { get; } 
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<string>> PropertyChoices { get; }
    IReadOnlyDictionary<INamedTypeSymbol, INamedTypeSymbol> ImplementationChoices { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> ImplementationCollectionChoices { get; }
    IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> InterceptorChoices { get; }
    
    bool IsSyncTransient(INamedTypeSymbol type);
    bool IsAsyncTransient(INamedTypeSymbol type);
    bool IsContainerInstance(ITypeSymbol type);
    bool IsTransientScopeInstance(ITypeSymbol type);
    bool IsScopeInstance(ITypeSymbol type);
    bool IsTransientScopeRoot(ITypeSymbol type);
    bool IsScopeRoot(ITypeSymbol type);
    bool IsComposite(INamedTypeSymbol implementationType);
    bool HasComposite(INamedTypeSymbol interfaceType);
    INamedTypeSymbol? GetCompositeFor(INamedTypeSymbol interfaceType);
    bool IsDecorator(INamedTypeSymbol implementationType);
    bool HasDecorators(INamedTypeSymbol interfaceType);
    ImmutableArray<INamedTypeSymbol> GetDecoratorsFor(INamedTypeSymbol interfaceType);
    int GetDecorationOrdinal(INamedTypeSymbol decoratorType);
    (INamedTypeSymbol Type, IMethodSymbol Method)? GetInitializerFor(INamedTypeSymbol implementationType);
    bool ImplementationConsidered(INamedTypeSymbol implementationType);
    ImmutableArray<INamedTypeSymbol> GetAllImplementingTypes(INamedTypeSymbol type);
}

internal abstract class CurrentlyConsideredTypesBase : ICurrentlyConsideredTypes
{
    private readonly IReadOnlyList<ITypesFromAttributesBase> _typesFromAttributes;
    private readonly ImplementationCache _implementationTypeSetCache;
    private readonly Lazy<IImmutableSet<INamedTypeSymbol>> _compositeAbstractionInterfaces;
    private readonly Lazy<IImmutableSet<INamedTypeSymbol>> _decoratorAbstractionInterfaces;

    protected CurrentlyConsideredTypesBase(
        IReadOnlyList<ITypesFromAttributesBase> typesFromAttributes,
        ImplementationCache implementationCache)
    {
        _typesFromAttributes = typesFromAttributes;
        _implementationTypeSetCache = implementationCache;
        IImmutableSet<INamedTypeSymbol> allImplementations = ImmutableHashSet<INamedTypeSymbol>.Empty;

        foreach (var types in typesFromAttributes)
        {
            if (types.FilterAllImplementations)
                allImplementations = ImmutableHashSet<INamedTypeSymbol>.Empty;
            else
            {
                allImplementations = allImplementations.Except(
                    types.FilterImplementation.OfType<INamedTypeSymbol>());
                allImplementations = types.FilterAssemblyImplementations.Aggregate(
                    allImplementations, 
                    (current, assembly) => current.Except(implementationCache.ForAssembly(assembly)));
            }

            if (types.AllImplementations)
            {
                allImplementations = implementationCache.All;
            }
            else
            {
                allImplementations = allImplementations.Union(
                    types.Implementation.OfType<INamedTypeSymbol>());
                allImplementations = types.AssemblyImplementations.Aggregate(
                    allImplementations, 
                    (current, assembly) => current.Union(implementationCache.ForAssembly(assembly)));
            }
        }

        AllConsideredImplementations = ImmutableHashSet.CreateRange<INamedTypeSymbol>(
            CustomSymbolEqualityComparer.Default,
            allImplementations.Select(t => t.UnboundIfGeneric()));
        
        _compositeAbstractionInterfaces = new Lazy<IImmutableSet<INamedTypeSymbol>>(() =>
        {
            var result = ImmutableHashSet<INamedTypeSymbol>.Empty;
            foreach (var types in _typesFromAttributes)
            {
                result = result.Except(types.FilterCompositeAbstraction.OfType<INamedTypeSymbol>().Select(c => c.UnboundIfGeneric()));
                result = result.Union(types.CompositeAbstraction.OfType<INamedTypeSymbol>().Select(c => c.UnboundIfGeneric()));
            }
            return result;
        });

        var constructorChoices = new Dictionary<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>>(CustomSymbolEqualityComparer.Default);
        
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
        
        _decoratorAbstractionInterfaces = new Lazy<IImmutableSet<INamedTypeSymbol>>(() =>
        {
            var result = ImmutableHashSet<INamedTypeSymbol>.Empty;
            foreach (var types in _typesFromAttributes)
            {
                result = result.Except(types.FilterDecoratorAbstraction.OfType<INamedTypeSymbol>().Select(c => c.UnboundIfGeneric()));
                result = result.Union(types.DecoratorAbstraction.OfType<INamedTypeSymbol>().Select(c => c.UnboundIfGeneric()));
            }
            return result;
        });
        
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
            foreach (var type in typesFromAttribute.FilterImplementationChoices.OfType<INamedTypeSymbol>())
                implementationChoices.Remove(type);

            foreach (var (type, choice) in typesFromAttribute.ImplementationChoices)
                implementationChoices[type.UnboundIfGeneric()] = choice;
        }

        ImplementationChoices = implementationChoices;

        var implementationCollectionChoices =
            new Dictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>>(CustomSymbolEqualityComparer.Default);
        
        foreach (var typesFromAttribute in typesFromAttributes)
        {
            foreach (var type in typesFromAttribute.FilterImplementationCollectionChoices.OfType<INamedTypeSymbol>())
                implementationCollectionChoices.Remove(type);

            foreach (var (type, choice) in typesFromAttribute.ImplementationCollectionChoices)
                implementationCollectionChoices[type.UnboundIfGeneric()] = choice;
        }

        ImplementationCollectionChoices = implementationCollectionChoices;
        
        var injectionKeyAttributeTypes = ImmutableHashSet<INamedTypeSymbol>.Empty;
        foreach (var typesFromAttribute in typesFromAttributes)
        {
            injectionKeyAttributeTypes = injectionKeyAttributeTypes.Except(typesFromAttribute.FilterInjectionKeyAttributeTypes);
            injectionKeyAttributeTypes = injectionKeyAttributeTypes.Union(typesFromAttribute.InjectionKeyAttributeTypes);
        }
        
        InjectionKeyAttributeTypes = injectionKeyAttributeTypes;
        
        var injectionKeyChoices = 
            ImmutableHashSet<(ITypeSymbol KeyType, object KeyValue, INamedTypeSymbol ImplementationType)>.Empty;
        foreach (var types in typesFromAttributes)
        {
            injectionKeyChoices = injectionKeyChoices.Except(types.FilterInjectionKeyChoices);
            injectionKeyChoices = injectionKeyChoices.Union(types.InjectionKeyChoices);
        }
        
        InjectionKeyChoices = injectionKeyChoices;
        
        var decorationOrdinalAttributeTypes = ImmutableHashSet<INamedTypeSymbol>.Empty;
        foreach (var typesFromAttribute in typesFromAttributes)
        {
            decorationOrdinalAttributeTypes = decorationOrdinalAttributeTypes.Except(typesFromAttribute.FilterDecorationOrdinalAttributeTypes);
            decorationOrdinalAttributeTypes = decorationOrdinalAttributeTypes.Union(typesFromAttribute.DecorationOrdinalAttributeTypes);
        }
        
        DecorationOrdinalAttributeTypes = decorationOrdinalAttributeTypes;
        
        var decorationOrdinalChoices = 
            ImmutableHashSet<(INamedTypeSymbol DecorationImplementationType, int Ordinal)>.Empty;
        
        foreach (var types in typesFromAttributes)
        {
            decorationOrdinalChoices = decorationOrdinalChoices.Except(
                decorationOrdinalChoices.Where(t => 
                    types.FilterDecorationOrdinalChoices.Contains(t.DecorationImplementationType)));
            decorationOrdinalChoices = decorationOrdinalChoices.Union(types.DecorationOrdinalChoices);
        }
        
        DecorationOrdinalChoices = decorationOrdinalChoices;
        
        var interceptorChoices = 
            new Dictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>>(CustomSymbolEqualityComparer.Default);
        
        foreach (var types in typesFromAttributes)
        {
            foreach (var type in types.FilterInterceptorChoices)
                interceptorChoices.Remove(type.UnboundIfGeneric());

            foreach (var (type, choice) in types.InterceptorChoices)
                interceptorChoices[type.UnboundIfGeneric()] = choice;
        }
        
        InterceptorChoices = interceptorChoices;
    }

    public IImmutableSet<INamedTypeSymbol> AllConsideredImplementations { get; }
    public IImmutableSet<INamedTypeSymbol> InjectionKeyAttributeTypes { get; }
    public IImmutableSet<(ITypeSymbol KeyType, object KeyValue, INamedTypeSymbol ImplementationType)> InjectionKeyChoices { get; }
    public IImmutableSet<INamedTypeSymbol> DecorationOrdinalAttributeTypes { get; }
    private IImmutableSet<(INamedTypeSymbol DecorationImplementationType, int Ordinal)> DecorationOrdinalChoices { get; }

    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>> ImplementationToConstructorChoice { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>>> DecoratorSequenceChoices { get; }
    public IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), IReadOnlyList<INamedTypeSymbol>> GenericParameterSubstitutesChoices { get; }
    public IReadOnlyDictionary<(INamedTypeSymbol, ITypeParameterSymbol), INamedTypeSymbol> GenericParameterChoices { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<string>> PropertyChoices { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, INamedTypeSymbol> ImplementationChoices { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> ImplementationCollectionChoices { get; }
    public IReadOnlyDictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> InterceptorChoices { get; }
    
    private readonly ConcurrentDictionary<INamedTypeSymbol, bool> _implementationConsideration = 
        new(CustomSymbolEqualityComparer.Default);
    public bool ImplementationConsidered(INamedTypeSymbol implementationType)
    {
        return  _implementationConsideration.GetOrAdd(implementationType, Inner);

        bool Inner(INamedTypeSymbol type)
        {
            var unbound = type.UnboundIfGeneric();
            foreach (var typesFromAttributes in _typesFromAttributes.Reverse())
            {
                if (typesFromAttributes.Implementation.Contains(implementationType)
                    || typesFromAttributes.Implementation.Contains(unbound))
                    return true;
                if (typesFromAttributes.FilterImplementation.Contains(implementationType)
                    || typesFromAttributes.FilterImplementation.Contains(unbound))
                    return false;
                if (typesFromAttributes.AssemblyImplementations.Contains(implementationType.ContainingAssembly))
                    return true;
                if (typesFromAttributes.FilterAssemblyImplementations.Contains(implementationType.ContainingAssembly))
                    return false;
                if (typesFromAttributes.AllImplementations)
                    return true;
                if (typesFromAttributes.FilterAllImplementations)
                    return false;
            }

            return false;
        }
    }

    public bool IsSyncTransient(INamedTypeSymbol type) =>
        HasProperty(
            type,
            x => x.SyncTransientAbstraction,
            x => x.FilterSyncTransientAbstraction,
            x => x.SyncTransientImplementation,
            x => x.FilterSyncTransientImplementation)
        || HasProperty(
            type,
            x => x.TransientAbstraction,
            x => x.FilterTransientAbstraction,
            x => x.TransientImplementation,
            x => x.FilterTransientImplementation);

    public bool IsAsyncTransient(INamedTypeSymbol type) =>
        HasProperty(
            type,
            x => x.AsyncTransientAbstraction,
            x => x.FilterAsyncTransientAbstraction,
            x => x.AsyncTransientImplementation,
            x => x.FilterAsyncTransientImplementation)
        || HasProperty(
            type,
            x => x.TransientAbstraction,
            x => x.FilterTransientAbstraction,
            x => x.TransientImplementation,
            x => x.FilterTransientImplementation);

    public bool IsContainerInstance(ITypeSymbol type) =>
        HasProperty(
            type,
            x => x.ContainerInstanceAbstraction,
            x => x.FilterContainerInstanceAbstraction,
            x => x.ContainerInstanceImplementation,
            x => x.FilterContainerInstanceImplementation);

    public bool IsTransientScopeInstance(ITypeSymbol type) =>
        HasProperty(
            type,
            x => x.TransientScopeInstanceAbstraction,
            x => x.FilterTransientScopeInstanceAbstraction,
            x => x.TransientScopeInstanceImplementation,
            x => x.FilterTransientScopeInstanceImplementation);

    public bool IsScopeInstance(ITypeSymbol type) =>
        HasProperty(
            type,
            x => x.ScopeInstanceAbstraction,
            x => x.FilterScopeInstanceAbstraction,
            x => x.ScopeInstanceImplementation,
            x => x.FilterScopeInstanceImplementation);

    public bool IsTransientScopeRoot(ITypeSymbol type) =>
        HasProperty(
            type,
            x => x.TransientScopeRootAbstraction,
            x => x.FilterTransientScopeRootAbstraction,
            x => x.TransientScopeRootImplementation,
            x => x.FilterTransientScopeRootImplementation);

    public bool IsScopeRoot(ITypeSymbol type) =>
        HasProperty(
            type,
            x => x.ScopeRootAbstraction,
            x => x.FilterScopeRootAbstraction,
            x => x.ScopeRootImplementation,
            x => x.FilterScopeRootImplementation);
    
    private readonly ConcurrentDictionary<INamedTypeSymbol, bool> _isCompositeCache = 
        new(CustomSymbolEqualityComparer.Default);

    public bool IsComposite(INamedTypeSymbol implementationType)
    {
        return _isCompositeCache.GetOrAdd(implementationType.UnboundIfGeneric(), Inner);

        bool Inner(INamedTypeSymbol unbound) =>
            HasProperty(
                unbound,
                t => t.CompositeAbstraction,
                t => t.FilterCompositeAbstraction,
                _ => ImmutableHashSet<ITypeSymbol>.Empty,
                _ => ImmutableHashSet<ITypeSymbol>.Empty);
    }
    
    private readonly ConcurrentDictionary<INamedTypeSymbol, INamedTypeSymbol?> _interfaceToCompositeCache =
        new(CustomSymbolEqualityComparer.Default);

    public bool HasComposite(INamedTypeSymbol interfaceType) =>
        GetCompositeFor(interfaceType) is not null;

    public INamedTypeSymbol? GetCompositeFor(INamedTypeSymbol interfaceType)
    {
        return _interfaceToCompositeCache.GetOrAdd(interfaceType.UnboundIfGeneric(), FindComposite);

        INamedTypeSymbol? FindComposite(INamedTypeSymbol unboundInterface)
        {
            // Search all implementations for one that:
            // 1. Implements IComposite<unboundInterface> (or similar composite marker)
            // 2. Is considered a composite via configuration

            return _implementationTypeSetCache.All
                .Select(i => i.OriginalDefinition)
                .Where(i => i.AllInterfaces.Any(iface =>
                {
                    var unboundIface = iface.UnboundIfGeneric();
                    if (!_compositeAbstractionInterfaces.Value.Contains(unboundIface, CustomSymbolEqualityComparer.Default))
                        return false;
                    return iface.TypeArguments.FirstOrDefault() is INamedTypeSymbol typeArg
                           && CustomSymbolEqualityComparer.Default.Equals(typeArg.UnboundIfGeneric(), unboundInterface);
                }))
                .Select(i => i.UnboundIfGeneric())
                .Where(IsComposite)
                .Where(ImplementationConsidered)
                .FirstOrDefault();
        }
    }
    
    private readonly ConcurrentDictionary<INamedTypeSymbol, bool> _isDecoratorCache =
        new(CustomSymbolEqualityComparer.Default);

    public bool IsDecorator(INamedTypeSymbol implementationType)
    {
        return _isDecoratorCache.GetOrAdd(implementationType.UnboundIfGeneric(), Inner);

        bool Inner(INamedTypeSymbol unbound) =>
            HasProperty(
                unbound,
                t => t.DecoratorAbstraction,
                t => t.FilterDecoratorAbstraction,
                _ => ImmutableHashSet<ITypeSymbol>.Empty,
                _ => ImmutableHashSet<ITypeSymbol>.Empty);
    }
    
    private readonly ConcurrentDictionary<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>> _interfaceToDecoratorsCache =
        new(CustomSymbolEqualityComparer.Default);

    public bool HasDecorators(INamedTypeSymbol interfaceType) =>
        GetDecoratorsFor(interfaceType).Length > 0;

    public ImmutableArray<INamedTypeSymbol> GetDecoratorsFor(INamedTypeSymbol interfaceType)
    {
        return _interfaceToDecoratorsCache.GetOrAdd(interfaceType.UnboundIfGeneric(), FindDecorators);

        ImmutableArray<INamedTypeSymbol> FindDecorators(INamedTypeSymbol unboundInterface)
        {
            return _implementationTypeSetCache.All
                .Select(i => i.OriginalDefinition)
                .Where(i => i.AllInterfaces.Any(iface =>
                {
                    var unboundIface = iface.UnboundIfGeneric();
                    if (!_decoratorAbstractionInterfaces.Value.Contains(unboundIface, CustomSymbolEqualityComparer.Default))
                        return false;
                    return iface.TypeArguments.FirstOrDefault() is INamedTypeSymbol typeArg
                           && CustomSymbolEqualityComparer.Default.Equals(typeArg.UnboundIfGeneric(), unboundInterface);
                }))
                .Select(i => i.UnboundIfGeneric())
                .Where(IsDecorator)
                .Where(ImplementationConsidered)
                .ToImmutableArray();
        }
    }
    
    private readonly ConcurrentDictionary<INamedTypeSymbol, int> _decorationOrdinalCache =
        new(CustomSymbolEqualityComparer.Default);

    public int GetDecorationOrdinal(INamedTypeSymbol decoratorType)
    {
        return _decorationOrdinalCache.GetOrAdd(decoratorType.UnboundIfGeneric(), ComputeOrdinal);

        int ComputeOrdinal(INamedTypeSymbol unbound)
        {
            // First check DecorationOrdinalChoices (explicit configuration takes precedence)
            var choice = DecorationOrdinalChoices
                .FirstOrDefault(c => CustomSymbolEqualityComparer.Default.Equals(
                    c.DecorationImplementationType.UnboundIfGeneric(), unbound));
            if (choice != default)
                return choice.Ordinal;

            // Then check attributes on the type itself
            var originalDef = unbound.OriginalDefinition;
            foreach (var attribute in originalDef.GetAttributes())
            {
                if (DecorationOrdinalAttributeTypes.Any(a => CustomSymbolEqualityComparer.Default.Equals(a, attribute.AttributeClass))
                    && attribute.ConstructorArguments.Length > 0
                    && attribute.ConstructorArguments[0].Value is int ordinal)
                    return ordinal;
            }

            return 0; // Default ordinal
        }
    }

    private readonly ConcurrentDictionary<INamedTypeSymbol, (INamedTypeSymbol, IMethodSymbol)?> _initializerCache =
        new(CustomSymbolEqualityComparer.Default);

    public (INamedTypeSymbol Type, IMethodSymbol Method)? GetInitializerFor(INamedTypeSymbol implementationType)
    {
        return _initializerCache.GetOrAdd(implementationType.UnboundIfGeneric(), FindInitializer);

        (INamedTypeSymbol, IMethodSymbol)? FindInitializer(INamedTypeSymbol unbound)
        {
            var originalDef = unbound.OriginalDefinition;
            var allTypesAndInterfaces = originalDef.AllDerivedTypesAndSelf()
                .Select(t => t.UnboundIfGeneric())
                .ToImmutableHashSet(CustomSymbolEqualityComparer.Default);

            (INamedTypeSymbol, IMethodSymbol)? result = null;

            foreach (var types in _typesFromAttributes.Reverse())
            {
                foreach (var (initType, initMethod) in types.Initializers)
                {
                    var initUnbound = initType.UnboundIfGeneric();
                    if (initType is { TypeKind: TypeKind.Interface } or { TypeKind: not TypeKind.Interface, IsAbstract: true })
                    {
                        if (allTypesAndInterfaces.Contains(initUnbound))
                            return (initUnbound, initMethod);
                    }
                    else if (CustomSymbolEqualityComparer.Default.Equals(initUnbound, unbound))
                        return (initUnbound, initMethod);
                }
                
                foreach (var filterInit in types.FilterInitializers)
                {
                    var filterUnbound = filterInit.UnboundIfGeneric();
                    if (filterInit is { TypeKind: TypeKind.Interface } or { TypeKind: not TypeKind.Interface, IsAbstract: true })
                    {
                        if (allTypesAndInterfaces.Contains(filterUnbound))
                            return null;
                    }
                    else if (CustomSymbolEqualityComparer.Default.Equals(filterUnbound, unbound))
                        return null;
                }
            }

            return result;
        }
    }

    private bool HasProperty(ITypeSymbol type,
        Func<ITypesFromAttributesBase, IImmutableSet<ITypeSymbol>> propertyGivingAbstractTypesGetter, 
        Func<ITypesFromAttributesBase, IImmutableSet<ITypeSymbol>> filteredPropertyGivingAbstractTypesGetter,
        Func<ITypesFromAttributesBase, IImmutableSet<ITypeSymbol>> propertyGivingImplementationTypesGetter, 
        Func<ITypesFromAttributesBase, IImmutableSet<ITypeSymbol>> filteredPropertyGivingImplementationTypesGetter)
    {
        var unbound = type.UnboundIfGeneric();
        var lazyDerivedTypes = new Lazy<ImmutableHashSet<ITypeSymbol>>(() =>
            type is INamedTypeSymbol namedTypeSymbol
                ? namedTypeSymbol.OriginalDefinition.AllDerivedTypesAndSelf().Select(t =>
                    t.UnboundIfGeneric()).ToImmutableHashSet<ITypeSymbol>(CustomSymbolEqualityComparer.Default)
                : [type]);
        foreach (var typesFromAttributes in _typesFromAttributes.Reverse())
        {
            if (propertyGivingImplementationTypesGetter(typesFromAttributes).Any(i => CustomSymbolEqualityComparer.Default.Equals(i.UnboundIfGeneric(), unbound)))
                return true;
            if (filteredPropertyGivingImplementationTypesGetter(typesFromAttributes).Any(i => CustomSymbolEqualityComparer.Default.Equals(i.UnboundIfGeneric(), unbound)))
                return false;
            if (propertyGivingAbstractTypesGetter(typesFromAttributes).Any(i => lazyDerivedTypes.Value.Contains(i.UnboundIfGeneric())))
                return true;
            if (filteredPropertyGivingAbstractTypesGetter(typesFromAttributes).Any(i => lazyDerivedTypes.Value.Contains(i.UnboundIfGeneric())))
                return false;
        }
        return false;
    }
    
    private readonly ConcurrentDictionary<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>> _implementationMap =
        new(CustomSymbolEqualityComparer.Default);
    public ImmutableArray<INamedTypeSymbol> GetAllImplementingTypes(INamedTypeSymbol type)
    {
        return _implementationMap.GetOrAdd(type.UnboundIfGeneric(), Inner);

        ImmutableArray<INamedTypeSymbol> Inner(INamedTypeSymbol unbound)
        {
            if (unbound.TypeKind == TypeKind.Interface)
                return _implementationTypeSetCache.All
                    .Where(implementation => implementation.OriginalDefinitionIfUnbound().AllInterfaces.Any(i =>
                        CustomSymbolEqualityComparer.Default.Equals(i.UnboundIfGeneric(), unbound)))
                    .Select(implementation => implementation.UnboundIfGeneric())
                    .Distinct(CustomSymbolEqualityComparer.Default)
                    .OfType<INamedTypeSymbol>()
                    .Where(ImplementationConsidered)
                    .ToImmutableArray();

            return _implementationTypeSetCache.All
                .Where(implementation => implementation.OriginalDefinitionIfUnbound().AllBaseTypesAndSelf().Any(i =>
                    CustomSymbolEqualityComparer.Default.Equals(i.UnboundIfGeneric(), unbound)))
                .Select(implementation => implementation.UnboundIfGeneric())
                .Distinct(CustomSymbolEqualityComparer.Default)
                .OfType<INamedTypeSymbol>()
                .Where(ImplementationConsidered)
                .ToImmutableArray();
        }
    }

    private sealed class DecoratorSequenceMap
    {
        private readonly Dictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> _map = new(CustomSymbolEqualityComparer.Default);

        public void Add(INamedTypeSymbol decoratedType, IReadOnlyList<INamedTypeSymbol> decoratorSequence) => 
            _map[decoratedType] = decoratorSequence;

        public void Remove(INamedTypeSymbol decoratedType) =>
            _map.Remove(decoratedType);

        public bool Any => 
            _map.Count != 0;

        public Dictionary<INamedTypeSymbol, IReadOnlyList<INamedTypeSymbol>> ToReadOnlyDictionary() => 
            _map;
    }
}