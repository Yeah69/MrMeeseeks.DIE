using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Configuration;

internal abstract record Decoration(INamedTypeSymbol Type)
{
    internal sealed record Decorator(INamedTypeSymbol Type) : Decoration(Type);
    internal sealed record Interceptor(INamedTypeSymbol Type) : Decoration(Type);
}

internal enum ScopeLevel
{
    None,
    Scope,
    TransientScope,
    Container
}

[Flags]
internal enum DisposalType
{
    None = 0,
    Sync = 1,
    Async = 2
}

internal interface IContainerCheckTypeProperties : ICheckTypeProperties;

internal sealed class ContainerCheckTypeProperties : CheckTypeProperties, IContainerCheckTypeProperties, IContainerInstance
{
    internal ContainerCheckTypeProperties(
        IContainerCurrentlyConsideredTypes currentlyConsideredTypes, 
        IInjectablePropertyExtractor injectablePropertyExtractor,
        WellKnownTypes wellKnownTypes,
        ITypeParameterUtility typeParameterUtility) 
        : base(currentlyConsideredTypes, injectablePropertyExtractor, wellKnownTypes, typeParameterUtility)
    {
    }
}

internal interface IScopeCheckTypeProperties : ICheckTypeProperties;

internal sealed class ScopeCheckTypeProperties : CheckTypeProperties, IScopeCheckTypeProperties, ITransientScopeInstance
{
    internal ScopeCheckTypeProperties(
        IScopeCurrentlyConsideredTypes currentlyConsideredTypes, 
        
        IInjectablePropertyExtractor injectablePropertyExtractor,
        WellKnownTypes wellKnownTypes,
        ITypeParameterUtility typeParameterUtility) 
        : base(currentlyConsideredTypes, injectablePropertyExtractor, wellKnownTypes, typeParameterUtility)
    {
    }
}

internal interface ICheckTypeProperties
{
    DisposalType ShouldDisposalBeManaged(INamedTypeSymbol implementationType);
    ScopeLevel ShouldBeScopeRoot(INamedTypeSymbol implementationType);
    bool ShouldBeComposite(INamedTypeSymbol interfaceType);
    ScopeLevel GetScopeLevelFor(INamedTypeSymbol implementationType);
    INamedTypeSymbol? GetCompositeFor(INamedTypeSymbol interfaceType);
    IMethodSymbol? GetConstructorChoiceFor(INamedTypeSymbol implementationType);
    
    IReadOnlyList<Decoration> GetDecorationSequenceFor(INamedTypeSymbol interfaceType,
        INamedTypeSymbol implementationType);

    INamedTypeSymbol? MapToSingleFittingImplementation(INamedTypeSymbol type,
        InjectionKey? injectionKey);
    IReadOnlyList<INamedTypeSymbol> MapToImplementations(INamedTypeSymbol typeSymbol,
        InjectionKey? injectionKey);
    IReadOnlyDictionary<object, INamedTypeSymbol> MapToKeyedImplementations(INamedTypeSymbol typeSymbol,
        ITypeSymbol keyType);
    IReadOnlyDictionary<object, IReadOnlyList<INamedTypeSymbol>> MapToKeyedMultipleImplementations(
        INamedTypeSymbol typeSymbol,
        ITypeSymbol keyType);
    (INamedTypeSymbol Type, IMethodSymbol Initializer)? GetInitializerFor(INamedTypeSymbol implementationType);
    IReadOnlyList<IPropertySymbol>? GetPropertyChoicesFor(INamedTypeSymbol implementationType);

    InjectionKey? IdentifyInjectionKeyModification(ISymbol parameterOrProperty);
}

internal abstract class CheckTypeProperties : ICheckTypeProperties
{
    private readonly ICurrentlyConsideredTypes _currentlyConsideredTypes;
    private readonly IInjectablePropertyExtractor _injectablePropertyExtractor;
    private readonly ITypeParameterUtility _typeParameterUtility;
    private readonly WellKnownTypes _wellKnownTypes;
    
    private readonly Dictionary<INamedTypeSymbol, IDictionary<ITypeSymbol, ISet<object>>> _typeToKeyToValue = new();
    private readonly Dictionary<INamedTypeSymbol, int> _decorationToOrdinal;

    internal CheckTypeProperties(
        ICurrentlyConsideredTypes currentlyConsideredTypes,
        IInjectablePropertyExtractor injectablePropertyExtractor,
        WellKnownTypes wellKnownTypes,
        ITypeParameterUtility typeParameterUtility)
    {
        _currentlyConsideredTypes = currentlyConsideredTypes;
        _injectablePropertyExtractor = injectablePropertyExtractor;
        _typeParameterUtility = typeParameterUtility;
        _wellKnownTypes = wellKnownTypes;

        var injectionKeyMappings = currentlyConsideredTypes.AllConsideredImplementations
            .SelectMany(i => i.GetAttributes().Select(a => (i, a)))
            .Select(t =>
            {
                var (implementation, attribute) = t;
                if (!currentlyConsideredTypes.InjectionKeyAttributeTypes.Any(a =>
                        CustomSymbolEqualityComparer.Default.Equals(a, attribute.AttributeClass)))
                    return ((ITypeSymbol? KeyType, object? KeyValue, INamedTypeSymbol ImplementationType)?)null;
                return ( 
                    attribute.ConstructorArguments[0].Type,
                    attribute.ConstructorArguments[0].Value,
                    implementation);
            })
            .Concat(currentlyConsideredTypes.InjectionKeyChoices.Select(t => 
                (t.KeyType, t.KeyValue, t.ImplementationType) 
                as (ITypeSymbol? KeyType, object? KeyValue, INamedTypeSymbol ImplementationType)?));
        
        foreach (var injectionKeyMapping in injectionKeyMappings)
        {
            if (injectionKeyMapping is not
                {
                    KeyType: {} keyType, 
                    KeyValue: {} keyValue, 
                    ImplementationType: {} implementationType
                })
                continue;
            
            if (_typeToKeyToValue.TryGetValue(implementationType, out var keyToValue))
            {
                if (keyToValue.TryGetValue(keyType, out var values))
                    values.Add(keyValue);
                else
                    keyToValue.Add(keyType, new HashSet<object>{keyValue});
            }
            else
                _typeToKeyToValue.Add(
                    implementationType.OriginalDefinition, 
                    new Dictionary<ITypeSymbol, ISet<object>>{{keyType, new HashSet<object>{keyValue}}});
        }
        
        _decorationToOrdinal = currentlyConsideredTypes.DecoratorTypes
            .SelectMany(d => d.GetAttributes().Select(a => (d, a)))
            .Select(t =>
            {
                var (decorator, attribute) = t;
                if (!currentlyConsideredTypes.DecorationOrdinalAttributeTypes.Any(a =>
                        CustomSymbolEqualityComparer.Default.Equals(a, attribute.AttributeClass)))
                    return ((INamedTypeSymbol DecorationImplementationType, int Ordinal)?)null;
                return (
                    DecorationImplementationType: decorator,
                    Ordinal: attribute.ConstructorArguments[0].Value is int ordinal ? ordinal : 0);
            })
            .Where(t => t is not null)
            .Select(t => t ?? throw new ImpossibleDieException())
            .Concat(currentlyConsideredTypes.DecorationOrdinalChoices)
            .ToDictionary(t => t.DecorationImplementationType, t => t.Ordinal);
    }
    
    public DisposalType ShouldDisposalBeManaged(INamedTypeSymbol implementationType)
    {
        if (implementationType.TypeKind is TypeKind.Struct)
            return DisposalType.None;

        var ret = DisposalType.None;
        
        if (_wellKnownTypes.IAsyncDisposable is not null 
            && implementationType.OriginalDefinitionIfUnbound().AllInterfaces.Contains(_wellKnownTypes.IAsyncDisposable)
            && !_currentlyConsideredTypes.AsyncTransientTypes.Contains(implementationType.UnboundIfGeneric()))
            ret |= DisposalType.Async;
        
        if (implementationType.OriginalDefinitionIfUnbound().AllInterfaces.Contains(_wellKnownTypes.IDisposable)
            && !_currentlyConsideredTypes.SyncTransientTypes.Contains(implementationType.UnboundIfGeneric()))
            ret |= DisposalType.Sync;
        
        return ret;
    }

    public ScopeLevel ShouldBeScopeRoot(INamedTypeSymbol implementationType)
    {
        if (_currentlyConsideredTypes.TransientScopeRootTypes.Contains(implementationType.UnboundIfGeneric()))
            return ScopeLevel.TransientScope;
        if (_currentlyConsideredTypes.ScopeRootTypes.Contains(implementationType.UnboundIfGeneric()))
            return ScopeLevel.Scope;
        return ScopeLevel.None;
    }

    public bool ShouldBeComposite(INamedTypeSymbol interfaceType) => _currentlyConsideredTypes.InterfaceToComposite.ContainsKey(interfaceType.UnboundIfGeneric());
    public ScopeLevel GetScopeLevelFor(INamedTypeSymbol implementationType)
    {
        var unbound = implementationType.UnboundIfGeneric();
        if (_currentlyConsideredTypes.ContainerInstanceTypes.Contains(unbound))
            return ScopeLevel.Container;
        if (_currentlyConsideredTypes.TransientScopeInstanceTypes.Contains(unbound))
            return ScopeLevel.TransientScope;
        if (_currentlyConsideredTypes.ScopeInstanceTypes.Contains(unbound))
            return ScopeLevel.Scope;
        return ScopeLevel.None;
    }

    public INamedTypeSymbol? GetCompositeFor(INamedTypeSymbol interfaceType)
    {
        var compositeImplementation = _currentlyConsideredTypes.InterfaceToComposite[interfaceType.UnboundIfGeneric()];
        var implementations = GetClosedImplementations(
            interfaceType, 
            [compositeImplementation], 
            true,
            true,
            false);
        
        var list = implementations.Take(2).ToList();
        
        return list.Count != 1 ? null : list[0];
    }

    public IMethodSymbol? GetConstructorChoiceFor(INamedTypeSymbol implementationType)
    {
        if (_currentlyConsideredTypes.ImplementationToConstructorChoice.TryGetValue(
                implementationType.UnboundIfGeneric(), out var constr))
            return constr;

        return implementationType switch
        {
            // If reference record and two constructors, decide for the constructor which isn't the copy-constructor
            { IsRecord: true, IsReferenceType: true, IsValueType: false, InstanceConstructors.Length: 2 } 
                when implementationType
                    .InstanceConstructors.SingleOrDefault(c =>
                        c.Parameters.Length != 1 ||
                        !CustomSymbolEqualityComparer.Default.Equals(c.Parameters[0].Type, implementationType)) 
                is { } constructor => constructor,
            
            // If value type and two constructors, decide for the constructor which isn't the parameterless constructor
            { IsRecord: true or false, IsReferenceType: false, IsValueType: true, InstanceConstructors.Length: 2 } 
                when implementationType.InstanceConstructors.SingleOrDefault(c => c.Parameters.Length > 0) 
                    is { } constructor => constructor,

            // If only one constructor, just choose it
            { InstanceConstructors.Length: 1 } when implementationType.InstanceConstructors.SingleOrDefault()
                is { } constructor => constructor,

            _ => null
        };
    }

    public IReadOnlyList<Decoration> GetDecorationSequenceFor(
        INamedTypeSymbol interfaceType, 
        INamedTypeSymbol implementationType)
    {
        IEnumerable<Decoration> sequence = Array.Empty<Decoration>();
        bool found = false;
        if (_currentlyConsideredTypes.DecoratorSequenceChoices.TryGetValue(interfaceType.UnboundIfGeneric(),
                out var sequenceMap))
        {
            if (sequenceMap.TryGetValue(implementationType.UnboundIfGeneric(), out var implementationSequence))
            {
                sequence = ToDecoration(implementationSequence);
                found = true;
            }
            else if (sequenceMap.TryGetValue(interfaceType.UnboundIfGeneric(), out var interfaceSequence))
            {
                sequence = ToDecoration(interfaceSequence);
                found = true;
            }
        }
        
        if (!found)
        {
            var unspecifiedSequence = _currentlyConsideredTypes.InterfaceToDecorators.TryGetValue(interfaceType.UnboundIfGeneric(), out var allDecorators)
                ? allDecorators
                : Enumerable.Empty<INamedTypeSymbol>();
            
            var implementationsBaseTypes = implementationType
                .AllDerivedTypesAndSelf()
                .Select(t => t.UnboundIfGeneric())
                .ToImmutableHashSet(CustomSymbolEqualityComparer.Default);

            unspecifiedSequence = unspecifiedSequence.Concat(
                _currentlyConsideredTypes.InterceptorChoices
                    .Where(kvp => kvp.Value.Any(imp => implementationsBaseTypes.Contains(imp.UnboundIfGeneric())))
                    .Select(kvp => kvp.Key));
            
            sequence = ToDecoration(unspecifiedSequence
                .OrderBy(d => _decorationToOrdinal.TryGetValue(d, out var ordinal) ? ordinal : 0));
        }

        return sequence
            .Select(d =>
            {
                switch (d)
                {
                    case Decoration.Decorator decorator:
                    {
                        var implementations = GetClosedImplementations(
                            interfaceType,
                            [decorator.Type],
                            true,
                            false,
                            true);
                
                        var list = implementations.Take(2).ToList();
                
                        return list.Count != 1 ? null : new Decoration.Decorator(list[0]);
                    }
                    case Decoration.Interceptor interceptor:
                        return interceptor;
                    default:
                        return (Decoration?) null;
                }
            })
            .OfType<Decoration>()
            .ToList();
        
        IEnumerable<Decoration> ToDecoration(IEnumerable<INamedTypeSymbol> source) =>
            source.Select(imp =>
            {
                if (_currentlyConsideredTypes.DecoratorTypes.Contains(imp, CustomSymbolEqualityComparer.Default))
                    return new Decoration.Decorator(imp);
                if (_currentlyConsideredTypes.InterceptorChoices.Keys.Contains(imp, CustomSymbolEqualityComparer.Default))
                    return new Decoration.Interceptor(imp);
                return (Decoration?) null;
            }).OfType<Decoration>();
    }

    public INamedTypeSymbol? MapToSingleFittingImplementation(INamedTypeSymbol type,
        InjectionKey? injectionKey)
    {
        var choice =
            _currentlyConsideredTypes.ImplementationChoices.TryGetValue(type.UnboundIfGeneric(), out var choice0)
                ? choice0
                : _currentlyConsideredTypes.ImplementationCollectionChoices.TryGetValue(type.UnboundIfGeneric(),
                      out var choices)
                  && choices.Count == 1 && choices[0] is { } choice1
                    ? choice1
                    : null;
        
        if (choice is not null)
        {
            var possibleChoices = FilterByInjectionKey(
                GetClosedImplementations(
                    type, 
                    [choice],
                    true, 
                    false, 
                    false), 
                injectionKey);
            
            var list = possibleChoices.Take(2).ToList();
            
            return list.Count == 1 ? list[0] : null;
        }

        if (type is { TypeKind: not TypeKind.Interface, IsAbstract: false, IsStatic: false })
        {
            if (_currentlyConsideredTypes.DecoratorTypes.Contains(type) ||
                _currentlyConsideredTypes.CompositeTypes.Contains(type))
                // if concrete type is decorator or composite then just shortcut
                return type;
            var possibleConcreteTypeImplementations = FilterByInjectionKey(
                GetClosedImplementations(
                    type,
                    [type],
                    true,
                    false,
                    false),
                injectionKey);
            
            var list = possibleConcreteTypeImplementations.Take(2).ToList();
            
            return list.Count == 1 ? list[0] : null;
        }

        var possibleImplementations = _currentlyConsideredTypes.ImplementationMap.TryGetValue(type.UnboundIfGeneric(), out var implementations) 
            ? FilterByInjectionKey(
                GetClosedImplementations(
                    type, 
                    [..implementations], 
                    true, 
                    false, 
                    false),
                injectionKey)
            : [];
        
        var list2 = possibleImplementations.Take(2).ToList();
        
        return list2.Count == 1 ? list2[0] : null;
    }

    public IReadOnlyList<INamedTypeSymbol> MapToImplementations(INamedTypeSymbol typeSymbol,
        InjectionKey? injectionKey)
    {
        if (_currentlyConsideredTypes
            .ImplementationCollectionChoices
            .TryGetValue(typeSymbol.UnboundIfGeneric(), out var choiceCollection))
        {
            return FilterByInjectionKey(
                GetClosedImplementations(
                    typeSymbol, 
                    choiceCollection, 
                    false, 
                    false, 
                    false),
                injectionKey).ToList();
        }
        
        return _currentlyConsideredTypes.ImplementationMap.TryGetValue(typeSymbol.UnboundIfGeneric(),
            out var implementations)
            ? FilterByInjectionKey(
                GetClosedImplementations(
                    typeSymbol,
                    [..implementations], 
                    false, 
                    false, 
                    false),
                injectionKey).ToList()
            : Array.Empty<INamedTypeSymbol>();
    }
    
    private IEnumerable<INamedTypeSymbol> FilterByInjectionKey(
        IEnumerable<INamedTypeSymbol> possibleChoices,
        InjectionKey? injectionKey) =>
        injectionKey is null
            ? possibleChoices
            : possibleChoices
                .Where(c => 
                    _typeToKeyToValue.TryGetValue(c.OriginalDefinition, out var keyToValue)
                    && keyToValue.TryGetValue(injectionKey.Type, out var values)
                    && values.Contains(injectionKey.Value));

    public IReadOnlyDictionary<object, IReadOnlyList<INamedTypeSymbol>> MapToKeyedMultipleImplementations(
        INamedTypeSymbol typeSymbol, ITypeSymbol keyType) =>
        MapToImplementations(typeSymbol, null)
            .SelectMany(i => _typeToKeyToValue.TryGetValue(i.OriginalDefinition, out var keyToValue)
                ? keyToValue.TryGetValue(keyType, out var values)
                    ? values.Select(v => (v, i))
                    : Enumerable.Empty<(object, INamedTypeSymbol)>()
                : [])
            .GroupBy(t => t.Item1)
            .ToDictionary(
                g => g.Key, 
                g => (IReadOnlyList<INamedTypeSymbol>) g.Select(t => t.Item2).ToList());

    public IReadOnlyDictionary<object, INamedTypeSymbol> MapToKeyedImplementations(INamedTypeSymbol typeSymbol,
        ITypeSymbol keyType) =>
        MapToKeyedMultipleImplementations(typeSymbol, keyType)
            .Where(kvp => kvp.Value.Count == 1)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value[0]);
    
    private IEnumerable<INamedTypeSymbol> GetClosedImplementations(
        INamedTypeSymbol targetType,
        IReadOnlyList<INamedTypeSymbol> rawImplementations,
        bool preferChoicesForOpenGenericParameters,
        bool chooseComposite,
        bool chooseDecorator)
    {
        var unboundTargetType = targetType.UnboundIfGeneric();
        foreach (var implementation in rawImplementations)
        {
            var unboundImplementation = implementation.UnboundIfGeneric();
            var originalImplementation = implementation.OriginalDefinitionIfUnbound();
            
            var isCompositeImplementation = _currentlyConsideredTypes.CompositeTypes.Contains(unboundImplementation);
            if (chooseComposite && !isCompositeImplementation || !chooseComposite && isCompositeImplementation)
                continue;
            var isDecoratorImplementation = _currentlyConsideredTypes.DecoratorTypes.Contains(unboundImplementation);
            if (chooseDecorator && !isDecoratorImplementation || !chooseDecorator && isDecoratorImplementation)
                continue;
            if (!chooseComposite 
                && !chooseDecorator 
                && !_currentlyConsideredTypes.AllConsideredImplementations.Contains(unboundImplementation))
                continue;
            if (!implementation.IsGenericType)
            {
                if (implementation
                        .AllDerivedTypesAndSelf()
                        .FirstOrDefault(t => CustomSymbolEqualityComparer.Default.Equals(t, targetType)) 
                    is not null)
                {
                    yield return implementation;
                }
                continue;
            }

            if (originalImplementation.TypeArguments.OfType<IErrorTypeSymbol>().Any())
            {
                // Ignore implementations with error type arguments
                continue;
            }
            
            var constructedFromImplementation = originalImplementation.ConstructedFrom;

            if (constructedFromImplementation.AllDerivedTypesAndSelf()
                    .FirstOrDefault(t =>
                        CustomSymbolEqualityComparer.Default.Equals(t.UnboundIfGeneric(), unboundTargetType))
                is not { } constructedFromTarget)
            {
                continue;
            }
            
            var typeArguments = new ITypeSymbol[implementation.TypeArguments.Length];
            
            var queue = ImmutableQueue.Create<(int Index, IImmutableSet<INamedTypeSymbol>)>();

            for (var i = 0; i < constructedFromImplementation.TypeArguments.Length; i++)
            {
                if (constructedFromTarget.TypeArguments.IndexOf(constructedFromImplementation.TypeArguments[i], CustomSymbolEqualityComparer.Default)
                    is var index and >= 0)
                {
                    typeArguments[i] = targetType.TypeArguments[index];
                    continue;
                }

                queue = queue.Enqueue((i, GetSubstitutes(i, constructedFromImplementation, unboundImplementation)));
            }
            
            if (queue.IsEmpty)
            {
                var constructed = implementation.ConstructedFrom.Construct(typeArguments);
                
                if (!_typeParameterUtility.CheckLegitimacyOfTypeArguments(constructed))
                    continue;
                
                if (constructed.AllDerivedTypesAndSelf().FirstOrDefault(i => CustomSymbolEqualityComparer.Default.Equals(i, targetType)) is not null)
                    yield return constructed;
                continue;
            }
            
            foreach (var closedImplementation in GetAllSubstituteCombinations(queue))
            {
                if (!_typeParameterUtility.CheckLegitimacyOfTypeArguments(closedImplementation))
                    continue;
                
                if (closedImplementation.AllDerivedTypesAndSelf().FirstOrDefault(i => CustomSymbolEqualityComparer.Default.Equals(i, targetType)) is not null)
                    yield return closedImplementation;
            }

            continue;
            
            

            IEnumerable<INamedTypeSymbol> GetAllSubstituteCombinations(
                IImmutableQueue<(int Index, IImmutableSet<INamedTypeSymbol> Substitutes)> currentSubstituteQueue)
            {
                if (currentSubstituteQueue.IsEmpty)
                    yield return implementation.ConstructedFrom.Construct(typeArguments);
                else
                {
                    currentSubstituteQueue = currentSubstituteQueue.Dequeue(out var tuple);
                    foreach (var substitute in tuple.Substitutes)
                    {
                        typeArguments[tuple.Index] = substitute;
                        foreach (var closedImplementation in GetAllSubstituteCombinations(currentSubstituteQueue))
                        {
                            yield return closedImplementation;
                        }
                    }
                }
            }
            
            IImmutableSet<INamedTypeSymbol> GetSubstitutes(
                int i,
                INamedTypeSymbol constructedFromImpl,
                INamedTypeSymbol unboundImpl)
            {
                if (preferChoicesForOpenGenericParameters)
                {
                    if (_currentlyConsideredTypes.GenericParameterChoices.TryGetValue(
                            (unboundImpl, constructedFromImpl.TypeParameters[i]), out var choice0))
                    {
                        return ImmutableHashSet.Create(choice0);
                    }
                    if (_currentlyConsideredTypes.GenericParameterSubstitutesChoices.TryGetValue(
                                 (unboundImpl, constructedFromImpl.TypeParameters[i]),
                                 out var subs0)
                             && subs0.Count == 1)
                    {
                        return ImmutableHashSet.Create(subs0[0]);
                    }
                }
                IImmutableSet<INamedTypeSymbol> substitutes1 = ImmutableHashSet.CreateRange<INamedTypeSymbol>(
                    CustomSymbolEqualityComparer.Default,
                    _currentlyConsideredTypes.GenericParameterSubstitutesChoices.TryGetValue(
                        (unboundImpl, constructedFromImpl.TypeParameters[i]), out var subs1)
                        ? subs1
                        : []);
                if (_currentlyConsideredTypes.GenericParameterChoices.TryGetValue(
                        (unboundImpl, constructedFromImpl.TypeParameters[i]), out var choice1))
                    substitutes1 = substitutes1.Add(choice1);
            
                return substitutes1;
            }
        }
    }

    public (INamedTypeSymbol Type, IMethodSymbol Initializer)? GetInitializerFor(INamedTypeSymbol implementationType)
    {
        if (_currentlyConsideredTypes.ImplementationToInitializer.TryGetValue(implementationType.UnboundIfGeneric(), out var tuple))
        {
            var abstractionType = implementationType
                .AllDerivedTypesAndSelf()
                .FirstOrDefault(t => CustomSymbolEqualityComparer.Default.Equals(t.UnboundIfGeneric(), tuple.Item1.UnboundIfGeneric()));

            var initializerMethod = implementationType
                .AllDerivedTypesAndSelf()
                .SelectMany(t => t.GetMembers(tuple.Item2.Name))
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m => CustomSymbolEqualityComparer.Default.Equals(m.OriginalDefinition, tuple.Item2.OriginalDefinition));
            
            return abstractionType is not null && initializerMethod is not null ? (abstractionType, initializerMethod) : null;
        }
        return null;
    }

    public IReadOnlyList<IPropertySymbol>? GetPropertyChoicesFor(INamedTypeSymbol implementationType)
    {
        var propertyChoicesNames = _currentlyConsideredTypes.PropertyChoices.TryGetValue(
            implementationType.UnboundIfGeneric(),
            out var properties)
            ? properties
            : null;

        if (propertyChoicesNames is null)
            return null;

        return _injectablePropertyExtractor
            .GetInjectableProperties(implementationType)
            .Where(p => propertyChoicesNames.Contains(p.Name))
            .ToList();
    }

    public InjectionKey? IdentifyInjectionKeyModification(ISymbol parameterOrProperty)
    {
        var intermediate = parameterOrProperty
            .GetAttributes()
            .Select(a => _currentlyConsideredTypes.InjectionKeyAttributeTypes.Any(ika =>
                             CustomSymbolEqualityComparer.Default.Equals(a.AttributeClass, ika))
                         && a.ConstructorArguments.Length == 1
                         && a.ConstructorArguments[0].Type is { } type
                         && a.ConstructorArguments[0].Value is { } value
                ? (type, value)
                : ((ITypeSymbol, object)?)null)
            .Where(t => t is not null)
            .ToArray();
        return intermediate.Length == 1 && intermediate[0] is { } ret
            ? new(ret.Item1, ret.Item2)
            : null;
    }
}