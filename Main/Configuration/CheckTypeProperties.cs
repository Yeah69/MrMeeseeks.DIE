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
    INamedTypeSymbol GetCompositeFor(INamedTypeSymbol interfaceType);
    IMethodSymbol? GetConstructorChoiceFor(INamedTypeSymbol implementationType);
    
    bool ShouldBeDecorated(INamedTypeSymbol interfaceType);
    IReadOnlyList<INamedTypeSymbol> GetSequenceFor(INamedTypeSymbol interfaceType, INamedTypeSymbol implementationType);
    
    IReadOnlyList<INamedTypeSymbol> MapToImplementations(ITypeSymbol typeSymbol);
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
            && !_currentlyConsideredTypes.AsyncTransientTypes.Contains(implementationType))
            return DisposalType.Async;
        
        if (implementationType.AllInterfaces.Contains(_wellKnownTypes.Disposable)
            && !_currentlyConsideredTypes.SyncTransientTypes.Contains(implementationType))
            return DisposalType.Sync;
        
        return DisposalType.None;
    }

    public ScopeLevel ShouldBeScopeRoot(INamedTypeSymbol implementationType)
    {
        if (_currentlyConsideredTypes.TransientScopeRootTypes.Contains(implementationType)) return ScopeLevel.TransientScope;
        return _currentlyConsideredTypes.ScopeRootTypes.Contains(implementationType) ? ScopeLevel.Scope : ScopeLevel.None;
    }

    public bool ShouldBeComposite(INamedTypeSymbol interfaceType) => _currentlyConsideredTypes.InterfaceToComposite.ContainsKey(interfaceType);
    public ScopeLevel GetScopeLevelFor(INamedTypeSymbol implementationType)
    {
        if (_currentlyConsideredTypes.ContainerInstanceTypes.Contains(implementationType))
            return ScopeLevel.Container;
        if (_currentlyConsideredTypes.TransientScopeInstanceTypes.Contains(implementationType))
            return ScopeLevel.TransientScope;
        if (_currentlyConsideredTypes.ScopeInstanceTypes.Contains(implementationType))
            return ScopeLevel.Scope;
        return ScopeLevel.None;
    }

    public INamedTypeSymbol GetCompositeFor(INamedTypeSymbol interfaceType) => _currentlyConsideredTypes.InterfaceToComposite[interfaceType];
    public IMethodSymbol? GetConstructorChoiceFor(INamedTypeSymbol implementationType)
    {
        if (implementationType.Constructors.Length == 1 
            && implementationType.Constructors.SingleOrDefault() is { } constructor)
            return constructor;

        return _currentlyConsideredTypes.ImplementationToConstructorChoice.TryGetValue(implementationType, out var constr) 
            ? constr : 
            null;
    }
    
    public bool ShouldBeDecorated(INamedTypeSymbol interfaceType) => _currentlyConsideredTypes.InterfaceToDecorators.ContainsKey(interfaceType);

    public IReadOnlyList<INamedTypeSymbol> GetSequenceFor(INamedTypeSymbol interfaceType, INamedTypeSymbol implementationType)
    {
        if (_currentlyConsideredTypes.ImplementationSequenceChoices.TryGetValue(implementationType, out var implementationSequence))
            return implementationSequence;
        if (_currentlyConsideredTypes.InterfaceSequenceChoices.TryGetValue(interfaceType, out var interfaceSequence))
            return interfaceSequence;
        if (_currentlyConsideredTypes.InterfaceToDecorators.TryGetValue(interfaceType, out var allDecorators)
            && allDecorators.Count == 1)
            return allDecorators;
        throw new Exception("Couldn't find unambiguous sequence of decorators");
    }
    
    public IReadOnlyList<INamedTypeSymbol> MapToImplementations(ITypeSymbol typeSymbol) =>
        _currentlyConsideredTypes.ImplementationMap.TryGetValue(typeSymbol, out var implementations) 
            ? implementations 
            : new List<INamedTypeSymbol>();

    public (INamedTypeSymbol Type, IMethodSymbol Initializer)? GetInitializerFor(INamedTypeSymbol implementationType) =>
        _currentlyConsideredTypes.ImplementationToInitializer.TryGetValue(implementationType, out var tuple)
            ? tuple
            : null;
}