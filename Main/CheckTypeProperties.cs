namespace MrMeeseeks.DIE;

public interface ICheckTypeProperties
{
    bool ShouldBeManaged(INamedTypeSymbol implementationType);
    bool ShouldBeSingleInstance(INamedTypeSymbol implementationType);
    bool ShouldBeScopedInstance(INamedTypeSymbol implementationType);
    bool ShouldBeScopeRoot(INamedTypeSymbol implementationType);
}

internal class CheckTypeProperties : ICheckTypeProperties
{
    private readonly IImmutableSet<ISymbol?> _transientTypes;
    private readonly IImmutableSet<ISymbol?> _singleInstanceTypes;
    private readonly IImmutableSet<ISymbol?> _scopedInstanceTypes;
    private readonly IImmutableSet<ISymbol?> _scopeRootTypes;

    public CheckTypeProperties(
        ITypesFromTypeAggregatingAttributes typesFromTypeAggregatingAttributes,
        IGetSetOfTypesWithProperties getSetOfTypesWithProperties)
    {
        _transientTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.Transient);
        _singleInstanceTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.SingleInstance);
        _scopedInstanceTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.ScopedInstance);
        _scopeRootTypes = getSetOfTypesWithProperties.Get(typesFromTypeAggregatingAttributes.ScopeRoot);
    }

    public bool ShouldBeManaged(INamedTypeSymbol implementationType) => !_transientTypes.Contains(implementationType);
    public bool ShouldBeSingleInstance(INamedTypeSymbol implementationType) => _singleInstanceTypes.Contains(implementationType);
    public bool ShouldBeScopedInstance(INamedTypeSymbol implementationType) => _scopedInstanceTypes.Contains(implementationType);
    public bool ShouldBeScopeRoot(INamedTypeSymbol implementationType) => _scopeRootTypes.Contains(implementationType);
}