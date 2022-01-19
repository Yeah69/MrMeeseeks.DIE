namespace MrMeeseeks.DIE;

internal enum ScopeLevel
{
    None,
    Scope,
    TransientScope,
    Container
}

internal interface ICheckTypeProperties
{
    bool ShouldBeManaged(INamedTypeSymbol implementationType);
    ScopeLevel ShouldBeScopeRoot(INamedTypeSymbol implementationType);
    bool ShouldBeComposite(INamedTypeSymbol interfaceType);
    ScopeLevel GetScopeLevelFor(INamedTypeSymbol implementationType);
    INamedTypeSymbol GetCompositeFor(INamedTypeSymbol interfaceType);
    bool IsComposite(INamedTypeSymbol implementationType);
    IMethodSymbol? GetConstructorChoiceFor(INamedTypeSymbol implementationType);
}

internal class CheckTypeProperties : ICheckTypeProperties
{
    private readonly IImmutableSet<ISymbol?> _transientTypes;
    private readonly IImmutableSet<ISymbol?> _containerInstanceTypes;
    private readonly IImmutableSet<ISymbol?> _transientScopeInstanceTypes;
    private readonly IImmutableSet<ISymbol?> _scopeInstanceTypes;
    private readonly IImmutableSet<ISymbol?> _transientScopeRootTypes;
    private readonly IImmutableSet<ISymbol?> _scopeRootTypes;
    private readonly IImmutableSet<ISymbol?> _compositeTypes;
    private readonly Dictionary<ISymbol?, INamedTypeSymbol> _interfaceToComposite;
    private readonly Dictionary<INamedTypeSymbol, IMethodSymbol> _implementationToConstructorChoice;

    internal CheckTypeProperties(
        ITypesFromTypeAggregatingAttributes typesFromTypeAggregatingAttributes,
        IGetAssemblyAttributes getAssemblyAttributes,
        WellKnownTypes wellKnownTypes,
        IGetSetOfTypesWithProperties getSetOfTypesWithProperties)
    {
        _transientTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.Transient);
        _containerInstanceTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.ContainerInstance);
        _transientScopeInstanceTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.TransientScopeInstance);
        _scopeInstanceTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.ScopeInstance);
        _transientScopeRootTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.TransientScopeRoot);
        _scopeRootTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.ScopeRoot);
        _compositeTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.Composite);
        _interfaceToComposite = _compositeTypes
            .OfType<INamedTypeSymbol>()
            .GroupBy(nts =>
            {
                var namedTypeSymbol = nts.AllInterfaces
                    .Single(t =>
                        typesFromTypeAggregatingAttributes.Composite.Contains(t.OriginalDefinition, SymbolEqualityComparer.Default));
                return namedTypeSymbol.TypeArguments.First();
            }, SymbolEqualityComparer.Default)
            .Where(g => g.Count() == 1)
            .ToDictionary(g => g.Key, g => g.Single(), SymbolEqualityComparer.Default);
        
        _implementationToConstructorChoice = getAssemblyAttributes
            .AllAssemblyAttributes
            .Concat(getAssemblyAttributes
                .AllAssemblyAttributes
                .Where(ad =>
                    ad.AttributeClass?.Equals(wellKnownTypes.SpyConstructorChoiceAggregationAttribute,
                        SymbolEqualityComparer.Default) ?? false)
                .SelectMany(ad => ad
                    .ConstructorArguments
                    .Where(ca => ca.Kind == TypedConstantKind.Enum)
                    .Select(ca => ca.Value as INamedTypeSymbol)
                    .OfType<INamedTypeSymbol>()
                    .SelectMany(e => e.GetAttributes())))
            .Where(ad =>
                ad.AttributeClass?.Equals(wellKnownTypes.ConstructorChoiceAttribute,
                    SymbolEqualityComparer.Default) ?? false)
            .Select(ad =>
            {
                if (ad.ConstructorArguments.Length < 2)
                    return null;
                var implementationType = ad.ConstructorArguments[0].Value as INamedTypeSymbol;
                var parameterTypes = ad
                    .ConstructorArguments[1]
                    .Values
                    .Select(tc => tc.Value)
                    .OfType<INamedTypeSymbol>()
                    .ToList();

                if (implementationType is { })
                {
                    var constructorChoice = implementationType
                        .Constructors
                        .Where(c => c.Parameters.Length == parameterTypes.Count)
                        .SingleOrDefault(c => c.Parameters.Select(p => p.Type)
                            .Zip(parameterTypes, (pLeft, pRight) => pLeft.Equals(pRight, SymbolEqualityComparer.Default))
                            .All(b => b));
                    return constructorChoice is { } 
                        ? (implementationType, constructorChoice) 
                        : ((INamedTypeSymbol, IMethodSymbol)?) null;
                }

                return null;
            })
            .OfType<(INamedTypeSymbol, IMethodSymbol)>()
            .ToDictionary(t => t.Item1, t => t.Item2);
    }

    public bool ShouldBeManaged(INamedTypeSymbol implementationType) => !_transientTypes.Contains(implementationType);
    public ScopeLevel ShouldBeScopeRoot(INamedTypeSymbol implementationType)
    {
        if (_transientScopeRootTypes.Contains(implementationType)) return ScopeLevel.TransientScope;
        return _scopeRootTypes.Contains(implementationType) ? ScopeLevel.Scope : ScopeLevel.None;
    }

    public bool ShouldBeComposite(INamedTypeSymbol interfaceType) => _interfaceToComposite.ContainsKey(interfaceType);
    public ScopeLevel GetScopeLevelFor(INamedTypeSymbol implementationType)
    {
        if (_containerInstanceTypes.Contains(implementationType))
            return ScopeLevel.Container;
        if (_transientScopeInstanceTypes.Contains(implementationType))
            return ScopeLevel.TransientScope;
        if (_scopeInstanceTypes.Contains(implementationType))
            return ScopeLevel.Scope;
        return ScopeLevel.None;
    }

    public INamedTypeSymbol GetCompositeFor(INamedTypeSymbol interfaceType) => _interfaceToComposite[interfaceType];
    public bool IsComposite(INamedTypeSymbol implementationType) => _compositeTypes.Contains(implementationType);
    public IMethodSymbol? GetConstructorChoiceFor(INamedTypeSymbol implementationType)
    {
        if (implementationType.Constructors.Length == 1 
            && implementationType.Constructors.SingleOrDefault() is { } constructor)
            return constructor;

        return _implementationToConstructorChoice.TryGetValue(implementationType, out var constr) 
            ? constr : 
            null;
    }
}