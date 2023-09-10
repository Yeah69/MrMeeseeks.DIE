using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Configuration;

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

internal interface IContainerCheckTypeProperties : ICheckTypeProperties
{
}

internal class ContainerCheckTypeProperties : CheckTypeProperties, IContainerCheckTypeProperties, IContainerInstance
{
    internal ContainerCheckTypeProperties(
        IContainerCurrentlyConsideredTypes currentlyConsideredTypes, 
        IInjectablePropertyExtractor injectablePropertyExtractor,
        IContainerWideContext containerWideContext,
        ILocalDiagLogger localDiagLogger) 
        : base(currentlyConsideredTypes, injectablePropertyExtractor, containerWideContext, localDiagLogger)
    {
    }
}

internal interface IScopeCheckTypeProperties : ICheckTypeProperties
{
}

internal class ScopeCheckTypeProperties : CheckTypeProperties, IScopeCheckTypeProperties, ITransientScopeInstance
{
    internal ScopeCheckTypeProperties(
        IScopeCurrentlyConsideredTypes currentlyConsideredTypes, 
        
        IInjectablePropertyExtractor injectablePropertyExtractor,
        IContainerWideContext containerWideContext,
        ILocalDiagLogger localDiagLogger) 
        : base(currentlyConsideredTypes, injectablePropertyExtractor, containerWideContext, localDiagLogger)
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
    
    bool ShouldBeDecorated(INamedTypeSymbol interfaceType);
    IReadOnlyList<INamedTypeSymbol> GetSequenceFor(INamedTypeSymbol interfaceType, INamedTypeSymbol implementationType);

    INamedTypeSymbol? MapToSingleFittingImplementation(INamedTypeSymbol type);
    IReadOnlyList<INamedTypeSymbol> MapToImplementations(INamedTypeSymbol typeSymbol);
    IReadOnlyDictionary<object, INamedTypeSymbol> MapToKeyedImplementations(INamedTypeSymbol typeSymbol, ITypeSymbol keyType);
    IReadOnlyDictionary<object, IReadOnlyList<INamedTypeSymbol>> MapToKeyedMultipleImplementations(INamedTypeSymbol typeSymbol, ITypeSymbol keyType);
    (INamedTypeSymbol Type, IMethodSymbol Initializer)? GetInitializerFor(INamedTypeSymbol implementationType);
    IReadOnlyList<IPropertySymbol>? GetPropertyChoicesFor(INamedTypeSymbol implementationType);
}

internal abstract class CheckTypeProperties : ICheckTypeProperties
{
    private readonly ICurrentlyConsideredTypes _currentlyConsideredTypes;
    private readonly IInjectablePropertyExtractor _injectablePropertyExtractor;
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly WellKnownTypes _wellKnownTypes;
    
    private readonly IDictionary<INamedTypeSymbol, IDictionary<ITypeSymbol, ISet<object>>> _typeToKeyToValue = 
        new Dictionary<INamedTypeSymbol, IDictionary<ITypeSymbol, ISet<object>>>();

    internal CheckTypeProperties(
        ICurrentlyConsideredTypes currentlyConsideredTypes,
        IInjectablePropertyExtractor injectablePropertyExtractor,
        IContainerWideContext containerWideContext,
        ILocalDiagLogger localDiagLogger)
    {
        _currentlyConsideredTypes = currentlyConsideredTypes;
        _injectablePropertyExtractor = injectablePropertyExtractor;
        _localDiagLogger = localDiagLogger;
        _wellKnownTypes = containerWideContext.WellKnownTypes;

        var injectionKeyMappings = currentlyConsideredTypes.AllConsideredImplementations
            .SelectMany(i => i.GetAttributes().Select(a => (i, a)))
            .Select(t =>
            {
                var (implementation, attribute) = t;
                if (!currentlyConsideredTypes.InjectionKeyAttributeTypes.Any(a =>
                        CustomSymbolEqualityComparer.Default.Equals(a, attribute.AttributeClass)))
                    return ((INamedTypeSymbol, ITypeSymbol?, object?)?)null;
                return (implementation, 
                    attribute.ConstructorArguments[0].Type,
                    attribute.ConstructorArguments[0].Value);
            });
        
        foreach (var injectionKeyMapping in injectionKeyMappings)
        {
            if (injectionKeyMapping is not { Item1: {} implementationType, Item2: {} keyType, Item3: {} keyValue })
                continue;
            
            if (_typeToKeyToValue.TryGetValue(implementationType, out var keyToValue))
            {
                if (keyToValue.TryGetValue(keyType, out var values))
                    values.Add(keyValue);
                else
                    keyToValue.Add(keyType, new HashSet<object>{keyValue});
            }
            else
            {
                _typeToKeyToValue.Add(implementationType, new Dictionary<ITypeSymbol, ISet<object>>{{keyType, new HashSet<object>{keyValue}}});
            }
        }
    }
    
    public DisposalType ShouldDisposalBeManaged(INamedTypeSymbol implementationType)
    {
        if (implementationType.TypeKind is TypeKind.Struct or TypeKind.Structure)
            return DisposalType.None;

        var ret = DisposalType.None;
        
        if (implementationType.AllInterfaces.Contains(_wellKnownTypes.IAsyncDisposable)
            && !_currentlyConsideredTypes.AsyncTransientTypes.Contains(implementationType.UnboundIfGeneric()))
            ret |= DisposalType.Async;
        
        if (implementationType.AllInterfaces.Contains(_wellKnownTypes.IDisposable)
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
            ImmutableHashSet.Create<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default, compositeImplementation), 
            true,
            true,
            false);
        if (implementations.Count != 1)
            return null;

        return implementations[0];
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
    
    public bool ShouldBeDecorated(INamedTypeSymbol interfaceType) => _currentlyConsideredTypes.InterfaceToDecorators.ContainsKey(interfaceType.UnboundIfGeneric());

    public IReadOnlyList<INamedTypeSymbol> GetSequenceFor(INamedTypeSymbol interfaceType, INamedTypeSymbol implementationType)
    {
        IEnumerable<INamedTypeSymbol> sequence = Array.Empty<INamedTypeSymbol>();
        bool found = false;
        if (_currentlyConsideredTypes.DecoratorSequenceChoices.TryGetValue(interfaceType.UnboundIfGeneric(),
                out var sequenceMap))
        {
            if (sequenceMap.TryGetValue(implementationType.UnboundIfGeneric(), out var implementationSequence))
            {
                sequence = implementationSequence;
                found = true;
            }
            else if (sequenceMap.TryGetValue(interfaceType.UnboundIfGeneric(), out var interfaceSequence))
            {
                sequence = interfaceSequence;
                found = true;
            }
        }
        
        if (!found && _currentlyConsideredTypes.InterfaceToDecorators.TryGetValue(interfaceType.UnboundIfGeneric(), out var allDecorators)
                 && allDecorators.Count == 1)
            sequence = allDecorators;
        
        return sequence
            .Select(imp =>
            {
                var implementations = GetClosedImplementations(
                    interfaceType,
                    ImmutableHashSet.Create<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default, imp),
                    true,
                    false,
                    true);
                if (implementations.Count != 1)
                    return null;
                return implementations[0];
            })
            .OfType<INamedTypeSymbol>()
            .ToList();
    }

    public INamedTypeSymbol? MapToSingleFittingImplementation(INamedTypeSymbol type)
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
            var possibleChoices = GetClosedImplementations(
                type, 
                ImmutableHashSet.Create<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default, choice), 
                true, 
                false, 
                false);
            return possibleChoices.Count == 1
                ? possibleChoices.FirstOrDefault()
                : null;
        }

        if (type is { TypeKind: not TypeKind.Interface, IsAbstract: false, IsStatic: false })
        {
            if (_currentlyConsideredTypes.DecoratorTypes.Contains(type) ||
                _currentlyConsideredTypes.CompositeTypes.Contains(type))
                // if concrete type is decorator or composite then just shortcut
                return type;
            var possibleConcreteTypeImplementations = GetClosedImplementations(
                type,
                ImmutableHashSet.Create<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default, type),
                true,
                false,
                false);
            return possibleConcreteTypeImplementations.Count == 1
                ? possibleConcreteTypeImplementations.FirstOrDefault()
                : null;
        }

        var possibleImplementations = _currentlyConsideredTypes.ImplementationMap.TryGetValue(type.UnboundIfGeneric(), out var implementations) 
            ? GetClosedImplementations(type, implementations, true, false, false)
            : Array.Empty<INamedTypeSymbol>();

        return possibleImplementations.Count == 1
            ? possibleImplementations.FirstOrDefault()
            : null;
    }

    public IReadOnlyList<INamedTypeSymbol> MapToImplementations(INamedTypeSymbol typeSymbol)
    {
        var isChoice =
            _currentlyConsideredTypes
                .ImplementationChoices
                .TryGetValue(typeSymbol.UnboundIfGeneric(), out var choice);
        
        var isCollectionChoice =
            _currentlyConsideredTypes
                .ImplementationCollectionChoices
                .TryGetValue(typeSymbol.UnboundIfGeneric(), out var choiceCollection);

        if (isChoice || isCollectionChoice)
        {
            var set = ImmutableHashSet.CreateRange<INamedTypeSymbol>(
                CustomSymbolEqualityComparer.Default,
                isCollectionChoice && choiceCollection is { }
                    ? choiceCollection
                    : Enumerable.Empty<INamedTypeSymbol>());
            if (isChoice && choice is { })
                set = set.Add(choice);
            return GetClosedImplementations(typeSymbol, set, false, false, false);
        }
        
        return _currentlyConsideredTypes.ImplementationMap.TryGetValue(typeSymbol.UnboundIfGeneric(),
            out var implementations)
            ? GetClosedImplementations(typeSymbol, implementations, false, false, false)
            : Array.Empty<INamedTypeSymbol>();
    }

    public IReadOnlyDictionary<object, IReadOnlyList<INamedTypeSymbol>> MapToKeyedMultipleImplementations(INamedTypeSymbol typeSymbol, ITypeSymbol keyType) =>
        MapToImplementations(typeSymbol)
            .SelectMany(i => _typeToKeyToValue.TryGetValue(i, out var keyToValue)
                ? keyToValue.TryGetValue(keyType, out var values)
                    ? values.Select(v => (v, i))
                    : Enumerable.Empty<(object, INamedTypeSymbol)>()
                : Enumerable.Empty<(object, INamedTypeSymbol)>())
            .GroupBy(t => t.Item1)
            .ToDictionary(
                g => g.Key, 
                g => (IReadOnlyList<INamedTypeSymbol>) g.Select(t => t.Item2).ToList());

    public IReadOnlyDictionary<object, INamedTypeSymbol> MapToKeyedImplementations(INamedTypeSymbol typeSymbol, ITypeSymbol keyType) =>
        MapToKeyedMultipleImplementations(typeSymbol, keyType)
            .Where(kvp => kvp.Value.Count == 1)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value[0]);

    private IReadOnlyList<INamedTypeSymbol> GetClosedImplementations(
        INamedTypeSymbol targetType,
        IImmutableSet<INamedTypeSymbol> rawImplementations,
        bool preferChoicesForOpenGenericParameters,
        bool chooseComposite,
        bool chooseDecorator)
    {
        var targetClosedGenericParameters = targetType
            .TypeArguments
            .OfType<INamedTypeSymbol>()
            .ToImmutableArray();
        var unboundTargetType = targetType.UnboundIfGeneric();
        var isTargetGeneric = targetType.IsGenericType;
        if (isTargetGeneric && targetType.TypeArguments.Any(tp => tp is not INamedTypeSymbol))
        {
            // Target type at this point should only have closed generic parameters
            _localDiagLogger.Error(
                ErrorLogData.ImpossibleException(new Guid("94B3BC00-9D37-4991-A66A-DDDF7C8402B6")),
                Location.None);
            throw new ImpossibleDieException(); 
        }

        var ret = new List<INamedTypeSymbol>();
        foreach (var implementation in rawImplementations)
        {
            var unboundImplementation = implementation.UnboundIfGeneric();
            var originalImplementation = implementation.OriginalDefinitionIfUnbound();

            if (!implementation.IsGenericType || implementation.TypeArguments.All(ta => ta is INamedTypeSymbol and not IErrorTypeSymbol))
            {
                if (originalImplementation.AllDerivedTypesAndSelf()
                    .FirstOrDefault(t =>
                        CustomSymbolEqualityComparer.Default.Equals(t, targetType)) is { })
                    AddImplementation(ret, implementation);
                continue;
            }

            if (originalImplementation.AllDerivedTypesAndSelf()
                    .FirstOrDefault(t =>
                        CustomSymbolEqualityComparer.Default.Equals(t.UnboundIfGeneric(), unboundTargetType)) is { } implementationsTarget)
            {
                var newTypeArguments = originalImplementation.TypeArguments
                    .Select(ta => ta switch
                    {
                        INamedTypeSymbol nts => nts,
                        ITypeParameterSymbol tps => ForTypeParameterSymbol(tps),
                        _ => ta
                    })
                    .ToArray();

                ITypeSymbol ForTypeParameterSymbol(ITypeParameterSymbol tps)
                {
                    if (implementationsTarget
                            .TypeArguments
                            .IndexOf(tps, CustomSymbolEqualityComparer.Default) is var index and >= 0)
                        return targetClosedGenericParameters[index];

                    if (preferChoicesForOpenGenericParameters)
                    {
                        if (_currentlyConsideredTypes.GenericParameterChoices.TryGetValue(
                                (unboundImplementation, tps), out var choice))
                            return choice;
                        if (_currentlyConsideredTypes.GenericParameterSubstitutesChoices.TryGetValue(
                                (unboundImplementation, tps), out var substitutes)
                            && substitutes.Count == 1)
                            return substitutes[0];
                    }

                    return tps;
                }


                if (newTypeArguments.Length == originalImplementation.TypeArguments.Length 
                    && newTypeArguments.All(ta => ta is INamedTypeSymbol))
                {
                    var closedImplementation = originalImplementation.Construct(newTypeArguments);
                    if (closedImplementation
                        .AllDerivedTypesAndSelf()
                        .Any(t => CustomSymbolEqualityComparer.Default.Equals(targetType, t)))
                    {
                        AddImplementation(ret, closedImplementation);
                    }
                }
                else if (newTypeArguments.Length == originalImplementation.TypeArguments.Length 
                         && newTypeArguments.All(ta => ta is INamedTypeSymbol or ITypeParameterSymbol))
                {
                    var openTypeParameters = newTypeArguments.OfType<ITypeParameterSymbol>().ToImmutableArray();
                    var queue = ImmutableQueue.CreateRange(openTypeParameters
                        .Select(tp =>
                        {
                            IImmutableSet<INamedTypeSymbol> substitutes = ImmutableHashSet.CreateRange<INamedTypeSymbol>(
                                CustomSymbolEqualityComparer.Default,
                                _currentlyConsideredTypes.GenericParameterSubstitutesChoices.TryGetValue(
                                    (unboundImplementation, tp), out var subs)
                                    ? subs
                                    : Array.Empty<INamedTypeSymbol>());
                            if (_currentlyConsideredTypes.GenericParameterChoices.TryGetValue(
                                    (unboundImplementation, tp), out var choice))
                                substitutes = substitutes.Add(choice);
                            return (tp, substitutes);
                        }));
                    if (queue.All(t => t.substitutes.Count > 0))
                    {
                        foreach (var combination in GetAllSubstituteCombinations(queue, ImmutableDictionary<ITypeParameterSymbol, INamedTypeSymbol>.Empty))
                        {
                            var veryNewTypeArguments = newTypeArguments
                                .Select(ta => ta switch
                                {
                                    INamedTypeSymbol nts => nts,
                                    ITypeParameterSymbol tps => combination.TryGetValue(tps, out var sub) ? sub : tps,
                                    _ => ta
                                })
                                .ToArray();
                            if (veryNewTypeArguments.Length == originalImplementation.TypeArguments.Length 
                                && veryNewTypeArguments.All(ta => ta is INamedTypeSymbol))
                            {
                                var closedImplementation = originalImplementation.Construct(veryNewTypeArguments);
                                if (closedImplementation
                                    .AllDerivedTypesAndSelf()
                                    .Any(t => CustomSymbolEqualityComparer.Default.Equals(targetType, t)))
                                {
                                    AddImplementation(ret, closedImplementation);
                                }
                            }
                        }

                        IEnumerable<IImmutableDictionary<ITypeParameterSymbol, INamedTypeSymbol>> GetAllSubstituteCombinations(
                                IImmutableQueue<(ITypeParameterSymbol, IImmutableSet<INamedTypeSymbol>)> currentSubstituteQueue,
                                IImmutableDictionary<ITypeParameterSymbol, INamedTypeSymbol> currentCombination)
                        {
                            if (currentSubstituteQueue.IsEmpty)
                                yield return currentCombination;
                            else
                            {
                                currentSubstituteQueue = currentSubstituteQueue.Dequeue(out var tuple);
                                foreach (var substitute in tuple.Item2)
                                {
                                    var newCurrentCombination = currentCombination.Add(tuple.Item1, substitute);
                                    foreach (var combination in GetAllSubstituteCombinations(currentSubstituteQueue, newCurrentCombination))
                                    {
                                        yield return combination;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return ret;

        void AddImplementation(IList<INamedTypeSymbol> implementations, INamedTypeSymbol added)
        {
            var unbound = added.UnboundIfGeneric();
            if (chooseComposite 
                && !chooseDecorator 
                && _currentlyConsideredTypes.CompositeTypes.Contains(unbound))
                implementations.Add(added);
            if (!chooseComposite 
                && chooseDecorator 
                && _currentlyConsideredTypes.DecoratorTypes.Contains(unbound))
                implementations.Add(added);
            if (!chooseComposite && !chooseDecorator
                && !_currentlyConsideredTypes.DecoratorTypes.Contains(unbound) 
                && !_currentlyConsideredTypes.CompositeTypes.Contains(unbound)
                && _currentlyConsideredTypes.AllConsideredImplementations.Contains(unbound)
                )
                implementations.Add(added);
        }
    }

    public (INamedTypeSymbol Type, IMethodSymbol Initializer)? GetInitializerFor(INamedTypeSymbol implementationType)
    {
        if (_currentlyConsideredTypes.ImplementationToInitializer.TryGetValue(implementationType.UnboundIfGeneric(), out var tuple))
        {
            return CustomSymbolEqualityComparer.Default.Equals(
                tuple.Item1.UnboundIfGeneric(), 
                implementationType.UnboundIfGeneric()) 
                ? (implementationType, tuple.Item2) 
                : tuple;
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
}