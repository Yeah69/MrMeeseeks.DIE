using MrMeeseeks.DIE.Extensions;

namespace MrMeeseeks.DIE.Configuration;

internal enum ScopeLevel
{
    None,
    Scope,
    TransientScope,
    Container
}

internal enum DisposalType
{
    None,
    Sync,
    Async
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
    (INamedTypeSymbol Type, IMethodSymbol Initializer)? GetInitializerFor(INamedTypeSymbol implementationType);
}

internal class CheckTypeProperties : ICheckTypeProperties
{
    private readonly ICurrentlyConsideredTypes _currentlyConsideredTypes;
    private readonly WellKnownTypes _wellKnownTypes;

    internal CheckTypeProperties(
        ICurrentlyConsideredTypes currentlyConsideredTypes,
        WellKnownTypes wellKnownTypes)
    {
        _currentlyConsideredTypes = currentlyConsideredTypes;
        _wellKnownTypes = wellKnownTypes;
    }
    
    public DisposalType ShouldDisposalBeManaged(INamedTypeSymbol implementationType)
    {
        if (implementationType.AllInterfaces.Contains(_wellKnownTypes.AsyncDisposable)
            && !_currentlyConsideredTypes.AsyncTransientTypes.Contains(implementationType.UnboundIfGeneric()))
            return DisposalType.Async;
        
        if (implementationType.AllInterfaces.Contains(_wellKnownTypes.Disposable)
            && !_currentlyConsideredTypes.SyncTransientTypes.Contains(implementationType.UnboundIfGeneric()))
            return DisposalType.Sync;
        
        return DisposalType.None;
    }

    public ScopeLevel ShouldBeScopeRoot(INamedTypeSymbol implementationType)
    {
        if (_currentlyConsideredTypes.TransientScopeRootTypes.Contains(implementationType.UnboundIfGeneric())) return ScopeLevel.TransientScope;
        return _currentlyConsideredTypes.ScopeRootTypes.Contains(implementationType.UnboundIfGeneric()) ? ScopeLevel.Scope : ScopeLevel.None;
    }

    public bool ShouldBeComposite(INamedTypeSymbol interfaceType) => _currentlyConsideredTypes.InterfaceToComposite.ContainsKey(interfaceType.UnboundIfGeneric());
    public ScopeLevel GetScopeLevelFor(INamedTypeSymbol implementationType)
    {
        if (_currentlyConsideredTypes.ContainerInstanceTypes.Contains(implementationType.UnboundIfGeneric()))
            return ScopeLevel.Container;
        if (_currentlyConsideredTypes.TransientScopeInstanceTypes.Contains(implementationType.UnboundIfGeneric()))
            return ScopeLevel.TransientScope;
        if (_currentlyConsideredTypes.ScopeInstanceTypes.Contains(implementationType.UnboundIfGeneric()))
            return ScopeLevel.Scope;
        return ScopeLevel.None;
    }

    public INamedTypeSymbol? GetCompositeFor(INamedTypeSymbol interfaceType)
    {
        var compositeImplementation = _currentlyConsideredTypes.InterfaceToComposite[interfaceType.UnboundIfGeneric()];
        var implementations = GetClosedImplementations(
            interfaceType, 
            new[] { compositeImplementation }, 
            true);
        if (implementations.Count != 1)
            return null;

        return implementations[0];
    }

    public IMethodSymbol? GetConstructorChoiceFor(INamedTypeSymbol implementationType)
    {
        var typeToChoseFrom = implementationType.OriginalDefinitionIfUnbound();
        if (typeToChoseFrom.Constructors.Length == 1 
            && typeToChoseFrom.Constructors.SingleOrDefault() is { } constructor)
            return constructor;

        return _currentlyConsideredTypes.ImplementationToConstructorChoice.TryGetValue(implementationType.UnboundIfGeneric(), out var constr) 
            ? constr : 
            null;
    }
    
    public bool ShouldBeDecorated(INamedTypeSymbol interfaceType) => _currentlyConsideredTypes.InterfaceToDecorators.ContainsKey(interfaceType.UnboundIfGeneric());

    public IReadOnlyList<INamedTypeSymbol> GetSequenceFor(INamedTypeSymbol interfaceType, INamedTypeSymbol implementationType)
    {
        IEnumerable<INamedTypeSymbol> sequence = Array.Empty<INamedTypeSymbol>();
        if (_currentlyConsideredTypes.DecoratorImplementationSequenceChoices.TryGetValue(implementationType.UnboundIfGeneric(), out var implementationSequence))
            sequence = implementationSequence;
        else if (_currentlyConsideredTypes.DecoratorInterfaceSequenceChoices.TryGetValue(interfaceType.UnboundIfGeneric(), out var interfaceSequence))
            sequence = interfaceSequence;
        else if (_currentlyConsideredTypes.InterfaceToDecorators.TryGetValue(interfaceType.UnboundIfGeneric(), out var allDecorators)
            && allDecorators.Count == 1)
            sequence = allDecorators;
        return sequence
            .Select(imp =>
            {
                var implementations = GetClosedImplementations(
                    interfaceType,
                    new[] { imp },
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
        var possibleImplementations = _currentlyConsideredTypes.ImplementationMap.TryGetValue(type.UnboundIfGeneric(), out var implementations) 
            ? GetClosedImplementations(type, implementations, true)
            : Array.Empty<INamedTypeSymbol>();

        return possibleImplementations.Count == 1
            ? possibleImplementations.FirstOrDefault()
            : null;
    }

    public IReadOnlyList<INamedTypeSymbol> MapToImplementations(INamedTypeSymbol typeSymbol) =>
        _currentlyConsideredTypes.ImplementationMap.TryGetValue(typeSymbol.UnboundIfGeneric(), out var implementations) 
            ? GetClosedImplementations(typeSymbol, implementations, false)
            : Array.Empty<INamedTypeSymbol>();

    private IReadOnlyList<INamedTypeSymbol> GetClosedImplementations(
        INamedTypeSymbol targetType,
        IReadOnlyList<INamedTypeSymbol> rawImplementations,
        bool preferChoicesForOpenGenericParameters)
    {
        var targetClosedGenericParameters = targetType
            .TypeArguments
            .OfType<INamedTypeSymbol>()
            .ToImmutableArray();
        var unboundTargetType = targetType.UnboundIfGeneric();
        var isTargetGeneric = targetType.IsGenericType;
        if (isTargetGeneric && targetType.TypeArguments.Any(tp => tp is not INamedTypeSymbol)) 
            throw new Exception("Target type at this point should only have closed generic parameters");

        var ret = new List<INamedTypeSymbol>();
        foreach (var implementation in rawImplementations)
        {
            if (!implementation.IsGenericType)
            {
                ret.Add(implementation);
                continue;
            }

            var unboundImplementation = implementation.UnboundIfGeneric();
            var originalImplementation = implementation.OriginalDefinitionIfUnbound();

            if (originalImplementation.AllDerivedTypes()
                    .FirstOrDefault(t =>
                        SymbolEqualityComparer.Default.Equals(t.UnboundIfGeneric(), unboundTargetType)) is { } implementationsTarget)
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
                            .IndexOf(tps, SymbolEqualityComparer.Default) is var index and >= 0)
                        return targetClosedGenericParameters[index];

                    if (preferChoicesForOpenGenericParameters)
                    {
                        if (_currentlyConsideredTypes.GenericParameterChoices.TryGetValue(
                                (unboundImplementation, tps), out var choice))
                            return choice;
                        if (_currentlyConsideredTypes.GenericParameterSubstitutes.TryGetValue(
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
                        .AllDerivedTypes()
                        .Any(t => SymbolEqualityComparer.Default.Equals(targetType, t)))
                    {
                        ret.Add(closedImplementation);
                    }
                }
                else if (newTypeArguments.Length == originalImplementation.TypeArguments.Length 
                         && newTypeArguments.All(ta => ta is INamedTypeSymbol or ITypeParameterSymbol))
                {
                    var openTypeParameters = newTypeArguments.OfType<ITypeParameterSymbol>().ToImmutableArray();
                    var queue = ImmutableQueue.CreateRange(openTypeParameters
                        .Select(tp =>
                        {
                            IImmutableSet<INamedTypeSymbol> substitutes = ImmutableHashSet.CreateRange(
                                _currentlyConsideredTypes.GenericParameterSubstitutes.TryGetValue(
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
                                    .AllDerivedTypes()
                                    .Any(t => SymbolEqualityComparer.Default.Equals(targetType, t)))
                                {
                                    ret.Add(closedImplementation);
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
    }

    public (INamedTypeSymbol Type, IMethodSymbol Initializer)? GetInitializerFor(INamedTypeSymbol implementationType)
    {
        if (_currentlyConsideredTypes.ImplementationToInitializer.TryGetValue(implementationType.UnboundIfGeneric(), out var tuple))
        {
            return SymbolEqualityComparer.Default.Equals(
                tuple.Item1.UnboundIfGeneric(), 
                implementationType.UnboundIfGeneric()) 
                ? (implementationType, tuple.Item2) 
                : tuple;
        }
        return null;
    }
}