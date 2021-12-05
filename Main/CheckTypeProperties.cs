namespace MrMeeseeks.DIE;

internal enum ScopeLevel
{
    None,
    Scope,
    TransientScope,
    SingleInstance
}

internal interface ICheckTypeProperties
{
    bool ShouldBeManaged(INamedTypeSymbol implementationType);
    bool ShouldBeScopeRoot(INamedTypeSymbol implementationType);
    bool ShouldBeComposite(INamedTypeSymbol interfaceType);
    ScopeLevel GetScopeLevelFor(INamedTypeSymbol implementationType);
    INamedTypeSymbol GetCompositeFor(INamedTypeSymbol interfaceType);
    bool IsComposite(INamedTypeSymbol implementationType);
}

internal class CheckTypeProperties : ICheckTypeProperties
{
    private readonly IImmutableSet<ISymbol?> _transientTypes;
    private readonly IImmutableSet<ISymbol?> _singleInstanceTypes;
    private readonly IImmutableSet<ISymbol?> _scopedInstanceTypes;
    private readonly IImmutableSet<ISymbol?> _scopeRootTypes;
    private readonly IImmutableSet<ISymbol?> _compositeTypes;
    private readonly Dictionary<ISymbol?,INamedTypeSymbol> _interfaceToComposite;

    internal CheckTypeProperties(
        ITypesFromTypeAggregatingAttributes typesFromTypeAggregatingAttributes,
        IGetSetOfTypesWithProperties getSetOfTypesWithProperties)
    {
        _transientTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.Transient);
        _singleInstanceTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.SingleInstance);
        _scopedInstanceTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.ScopedInstance);
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
    }

    public bool ShouldBeManaged(INamedTypeSymbol implementationType) => !_transientTypes.Contains(implementationType);
    public bool ShouldBeScopeRoot(INamedTypeSymbol implementationType) => _scopeRootTypes.Contains(implementationType);
    public bool ShouldBeComposite(INamedTypeSymbol interfaceType) => _interfaceToComposite.ContainsKey(interfaceType);
    public ScopeLevel GetScopeLevelFor(INamedTypeSymbol implementationType)
    {
        if (_singleInstanceTypes.Contains(implementationType))
            return ScopeLevel.SingleInstance;
        if (_scopedInstanceTypes.Contains(implementationType))
            return ScopeLevel.Scope;
        return ScopeLevel.None;
    }

    public INamedTypeSymbol GetCompositeFor(INamedTypeSymbol interfaceType) => _interfaceToComposite[interfaceType];
    public bool IsComposite(INamedTypeSymbol implementationType) => _compositeTypes.Contains(implementationType);
}